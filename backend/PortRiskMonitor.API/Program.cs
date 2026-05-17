// ============================================================
// PROGRAM.CS — Application entry point
// This file wires together the entire 3-tier application:
//   - Presentation layer: ASP.NET Core HTTP pipeline
//   - Business Logic layer: Application services
//   - Data Access layer: EF Core + SQLite
//
// TODO for implementors:
//   1. Replace SQLite with SQL Server for production
//      (change UseSqlite → UseSqlServer, update connection string)
//   2. Add ASP.NET Core Identity here when user auth is needed
//   3. Swap Serilog sinks to a log aggregator (e.g. Seq, Datadog)
// ============================================================

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.API.Filters;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Application.Services;
using PortRiskMonitor.Infrastructure.BerthOccupancy;
using PortRiskMonitor.Infrastructure.CustomsDwellTime;
using PortRiskMonitor.Infrastructure.Data;
using PortRiskMonitor.Infrastructure.Repositories;
using PortRiskMonitor.Infrastructure.RiskMonitor;
using PortRiskMonitor.Infrastructure.VesselDelayRate;
using PortRiskMonitor.Infrastructure.WeatherCondition;
using RiskMonitor.Repositories;
using Serilog;

// ── Storage note ─────────────────────────────────────────────────────────────
// Historical data is stored in the same SQLite DB using EF Core (KriDefinitions
// + KriReadings tables). To migrate to Postgres:
//   1. Change connection string in appsettings.json
//   2. Replace `options.UseSqlite(...)` with `options.UseNpgsql(...)`
//   3. Add the Npgsql.EntityFrameworkCore.PostgreSQL NuGet package
//   4. Run `dotnet ef migrations add PostgresMigration`
// Zero application-layer code needs to change.

// ── Serilog bootstrap logger (catches startup errors before full config) ──────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog full configuration ────────────────────────────────────────────
    // TODO: Add Serilog.Sinks.Seq for structured log viewing in development
    // TODO: In production consider Serilog.Sinks.ApplicationInsights
    builder.Host.UseSerilog((context, services, config) =>
    {
        config
            .ReadFrom.Configuration(context.Configuration) // reads from appsettings.json Serilog section
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/port-risk-monitor-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14
            );
    });

    // ── Database — Data Access Layer ─────────────────────────────────────────
    // NFR: Data Access — EF Core ORM, transactions scoped to single HTTP request
    // SQLite is used for PoC (zero config). Swap to SQL Server for production.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=port_risk_monitor.db";

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlite(connectionString);
        // TODO: Enable sensitive data logging only in Development
        // options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    });

    // ── Dependency Injection — Business Logic Layer ───────────────────────────
    // NFR: Memory Management — all services registered as Scoped (per-request lifetime)
    // NEVER use AddSingleton for stateful services — would cause cross-request data leakage
    // NEVER use AddSingleton for DbContext — EF Core is not thread-safe across requests

    // Repositories (Data Access Layer)
    builder.Services.AddScoped<IKriRepository, KriRepository>(); // IKriRepository = RiskMonitor.Repositories.IKriRepository
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<IRiskMonitorRepository, PortRiskMonitorRepo>();
    builder.Services.AddScoped<IVesselDelayRateRepo, VesselDelayRateRepo>();
    builder.Services.AddScoped<IBerthOccupancyRepo, BerthOccupancyRepo>();
    builder.Services.AddScoped<ICustomsDwellTimeRepo, CustomsDwellTimeRepo>();
    builder.Services.AddScoped<IWeatherConditionRepo, WeatherConditionRepo>();

    // Application Services (Business Logic Layer)
    builder.Services.AddScoped<IPortRiskMonitorService, PortRiskMonitorService>();

    // Indicator Services
    builder.Services.AddScoped<IBerthOccupancyService, BerthOccupancyService>();
    builder.Services.AddScoped<IVesselDelayRateService, VesselDelayRateService>();
    builder.Services.AddScoped<ICustomsDwellTimeService, CustomsDwellTimeService>();
    builder.Services.AddHttpClient<WeatherConditionService>();
    builder.Services.AddScoped<IWeatherConditionService, WeatherConditionService>();

    // TODO
    // NFR: Extensibility — Strategy Pattern for risk score calculation
    // To swap algorithm: change "RiskScoring:Strategy" in appsettings.json
    // No code recompilation needed — only config change
    //var strategyName = builder.Configuration["RiskScoring:Strategy"] ?? "DefaultWeighted";
    //builder.Services.AddScoped<IRiskScoreStrategy>(sp =>
    //{
    //    // TODO: Add more strategy implementations here as the system grows
    //    // Each new strategy is a new class — existing code is never modified
    //    return strategyName switch
    //    {
    //        "MaxRisk" => new MaxRiskStrategy(),       // Takes worst single KRI score
    //        "AverageRisk" => new AverageRiskStrategy(),   // Simple arithmetic mean
    //        _ => new DefaultWeightedStrategy() // Default: weighted sum
    //    };
    //});

    // NFR: Async Communication — BackgroundService for mock data generation
    // Runs in background thread, never blocks HTTP request handlers
    // TODO: Remove MockDataBackgroundService when real data sources are connected
    //builder.Services.AddHostedService<MockDataBackgroundService>();

    // ── FluentValidation ──────────────────────────────────────────────────────
    // TODO: Register all validators — FluentValidation will auto-scan the assembly
    //builder.Services.AddValidatorsFromAssemblyContaining<IKriService>();

    // ── CORS — allow React dev server ─────────────────────────────────────────
    // TODO: Lock down CORS origins for production (remove AllowAnyOrigin)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactDevPolicy", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173", // Vite dev server default
                    "http://localhost:3000"  // CRA fallback
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // needed for SSE (Server-Sent Events)
        });
    });

    // ── Controllers + Action Filters ─────────────────────────────────────────
    // NFR: Interceptors — BusinessLogicAuditFilter registered globally
    // Logs every controller action: class, method, user, timestamp, duration
    // Toggle via appsettings.json "Auditing:Enabled" — no recompile needed
    builder.Services.AddControllers(options =>
    {
        var auditingEnabled = builder.Configuration.GetValue<bool>("Auditing:Enabled");
        if (auditingEnabled)
        {
            options.Filters.Add<BusinessLogicAuditFilter>();
            Log.Information("Audit logging is ENABLED");
        }
        else
        {
            Log.Warning("Audit logging is DISABLED — set Auditing:Enabled=true in appsettings.json to enable");
        }
    });

    builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // TODO: Add XML comments for better Swagger documentation
        // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PortRiskMonitor.API.xml"));
        options.SwaggerDoc("v1", new() { Title = "Port Risk Monitor API", Version = "v1" });
    });

    // ── Build the app ─────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Auto-run EF migrations on startup ────────────────────────────────────
    // TODO: In production, run migrations as part of the deployment pipeline
    //       rather than on every startup — can cause issues with rolling deployments
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");

        // Seed KRI definitions + 30 days of hourly historical readings for development.
        // SeedData.SeedAsync is idempotent — it checks AnyAsync() first and skips if data exists.
        await SeedData.SeedAsync(db);
    }

    // ── HTTP Pipeline ─────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Port Risk Monitor v1");
            // Makes Swagger the default page in development
            options.RoutePrefix = string.Empty;
        });
    }

    app.UseSerilogRequestLogging(); // logs every HTTP request with timing

    app.UseCors("ReactDevPolicy");

    app.UseHttpsRedirection();

    // TODO: Add app.UseAuthentication() and app.UseAuthorization() when user accounts are added

    app.MapControllers();

    Log.Information("Port Risk Monitor API starting on {Environment}", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
