using Amazon.SimpleNotificationService;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using PortRiskMonitor.API.Exceptions;
using PortRiskMonitor.API.Filters;
using PortRiskMonitor.Application.BackgroundServices;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Application.Services;
using PortRiskMonitor.Infrastructure.Alerts;
using PortRiskMonitor.Infrastructure.Data;
using PortRiskMonitor.Infrastructure.Notifications;
using PortRiskMonitor.Infrastructure.PortStatus;
using PortRiskMonitor.Infrastructure.Repositories;
using PortRiskMonitor.Infrastructure.RiskMonitor;
using RiskMonitor.Repositories;
using RiskMonitor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
    {
        config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/port-risk-monitor-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14
            );
    });

    // NFR: Data Access — EF Core ORM; DbContext is scoped per HTTP request,
    // so each DB transaction starts and ends within a single request.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=port_risk_monitor.db";

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    // NFR: Memory Management — all services registered as Scoped (per-request lifetime).
    // No use-case state is stored in session; each request gets fresh instances.

    // Repositories (Data Access Layer)
    builder.Services.AddScoped<IAlertRepository, AlertRepository>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<IRiskMonitorRepository, PortRiskMonitorRepo>();
    builder.Services.AddScoped<IPortStatusRepo, PortStatusRepo>();

    builder.Services.AddSingleton<IWeatherSnapshotCache, WeatherSnapshotCache>();
    builder.Services.AddSingleton<IAisSnapshotCache, AisSnapshotCache>();

    // Application Services (Business Logic Layer)
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
    builder.Services.AddScoped<IDasboardService, DashboardService>();
    builder.Services.AddScoped<IThresholdSettingsService, ThresholdSettingsService>();
    builder.Services.AddScoped<IAlertingService, AlertingService>();

    builder.Services.AddSingleton<IFilterInputParser, FilterInputParser>();

    // NFR: Extensibility / Strategy — IAlertNotifier implementation is selected
    // via appsettings.json "Alerts:Sms:Enabled". New notifiers (e.g. email, Slack)
    // can be added without modifying existing code — only config changes needed.
    builder.Services.Configure<SmsAlertOptions>(
        builder.Configuration.GetSection(SmsAlertOptions.SectionName));

    var smsOptions = builder.Configuration.GetSection(SmsAlertOptions.SectionName).Get<SmsAlertOptions>()
        ?? new SmsAlertOptions();
    if (smsOptions.Enabled)
    {
        builder.Services.AddSingleton<IAmazonSimpleNotificationService>(_ =>
            new AmazonSimpleNotificationServiceClient(
                Amazon.RegionEndpoint.GetBySystemName(smsOptions.AwsRegion)));
        builder.Services.AddScoped<IAlertNotifier, AwsSnsAlertNotifier>();
        Log.Information("SMS alerts ENABLED via AWS SNS (region: {Region}, recipients: {Count})",
            smsOptions.AwsRegion, smsOptions.RecipientPhoneNumbers.Count);
    }
    else
    {
        builder.Services.AddScoped<IAlertNotifier, NullAlertNotifier>();
        Log.Information("SMS alerts DISABLED — using NullAlertNotifier (log-only)");
    }

    // NFR: Reactive / Async — background services run on separate threads,
    // never blocking HTTP request handlers.
    builder.Services.AddHostedService<AlertEvaluationBackgroundService>();

    builder.Services.AddHttpClient<WeatherFetcherService>();
    builder.Services.AddHostedService<WeatherFetcherService>();

    builder.Services.AddHttpClient<AisFetcherService>();
    builder.Services.AddHostedService<AisFetcherService>();


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

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // NFR: Cross-cutting / Interceptors — BusinessLogicAuditFilter logs every
    // controller action (class, method, user, timestamp, duration).
    // Toggle via appsettings.json "Auditing:Enabled" — no recompile needed.
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
        options.SwaggerDoc("v1", new() { Title = "Port Risk Monitor API", Version = "v1" });
    });

    var app = builder.Build();

    // Apply pending EF migrations on startup (PoC convenience)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
        await SeedData.SeedAsync(db);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Port Risk Monitor v1");
            options.RoutePrefix = string.Empty;
        });
    }

    app.UseExceptionHandler(_ => { });
    app.UseSerilogRequestLogging();
    app.UseCors("ReactDevPolicy");
    app.UseHttpsRedirection();
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
