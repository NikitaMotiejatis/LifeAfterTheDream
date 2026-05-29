using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.API.Extensions;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Application.Notifications.Options;
using PortRiskMonitor.Data.Alerts;
using PortRiskMonitor.Data.Data;
using PortRiskMonitor.Data.PortStatus;
using PortRiskMonitor.Data.Repositories;
using RiskMonitor.Repositories;
using RiskMonitor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(20);

        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;

        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

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
    builder.Services
        .AddScopedFromConfig<IAlertRepository>(builder.Configuration, "DynamicStrategies:IAlertRepository")
        .AddScopedFromConfig<IAuditLogRepository>(builder.Configuration, "DynamicStrategies:IAuditLogRepository")
        .AddScopedFromConfig<IRiskMonitorRepository>(builder.Configuration, "DynamicStrategies:IRiskMonitorRepository")
        .AddScopedFromConfig<IPortStatusRepo>(builder.Configuration, "DynamicStrategies:IPortStatusRepo");

    builder.Services
        .AddSingletonFromConfig<IWeatherSnapshotCache>(builder.Configuration, "DynamicStrategies:IWeatherSnapshotCache")
        .AddSingletonFromConfig<IAisSnapshotCache>(builder.Configuration, "DynamicStrategies:IAisSnapshotCache");

    // NFR: Extensibility / Strategy + Decorator — IAlertNotifier is selected at
    // runtime by ChannelDispatchingNotifier, which reads IOptionsMonitor every
    // call.
    builder.Services.Configure<NotificationOptions>(
        builder.Configuration.GetSection(NotificationOptions.SectionName));

    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        var loggerClass = builder.Configuration.ReadTypeFromConfig<IInterceptor>("DynamicStrategies:LoggerInterceptor");
        containerBuilder.RegisterType(loggerClass)
                        .As<IInterceptor>()
                        .AsSelf();

        // Add logging interceptor to business logic services
        {
            var businessServices = new[]
            {
                builder.Configuration.ReadTypeFromConfig<IAnalyticsService>("DynamicStrategies:IAnalyticsService"),
                builder.Configuration.ReadTypeFromConfig<IDasboardService>("DynamicStrategies:IDashboardService"),
                builder.Configuration.ReadTypeFromConfig<IThresholdSettingsService>("DynamicStrategies:IThresholdSettingsService"),
                builder.Configuration.ReadTypeFromConfig<INotificationService>("DynamicStrategies:INotificationService"),
                builder.Configuration.ReadTypeFromConfig<IAlertingService>("DynamicStrategies:IAlertingService"),
                builder.Configuration.ReadTypeFromConfig<IIndicatorService>("DynamicStrategies:IIndicatorService"),
            };

            foreach (var service in businessServices)
            {
                containerBuilder.RegisterType(service)
                                .AsImplementedInterfaces()
                                .InstancePerLifetimeScope()
                                .EnableInterfaceInterceptors()
                                .InterceptedBy(loggerClass);
            }
        }

        // Add logging interceptor to alert services
        {
            var alertNotifierDecorator = builder.Configuration.ReadTypeFromConfig<IAlertNotifier>("DynamicStrategies:AlertNotifierDecorator");
            containerBuilder.RegisterType(alertNotifierDecorator)
                            .As<IAlertNotifier>()
                            .InstancePerLifetimeScope()
                            .EnableInterfaceInterceptors()
                            .InterceptedBy(loggerClass);

            var alertNotifiers = new[]
            {
                (builder.Configuration.ReadTypeFromConfig<IAlertNotifier>("DynamicStrategies:NullAlertNotifier"), "Off"),
                (builder.Configuration.ReadTypeFromConfig<IAlertNotifier>("DynamicStrategies:TwilioSmsAlertNotifier"), "Twilio"),
                (builder.Configuration.ReadTypeFromConfig<IAlertNotifier>("DynamicStrategies:SmtpEmailAlertNotifier"), "Email"),
            };

            foreach (var (notifier, key) in alertNotifiers)
            {
                containerBuilder.RegisterType(notifier)
                                .Keyed<IAlertNotifier>(key)
                                .InstancePerLifetimeScope()
                                .EnableInterfaceInterceptors()
                                .InterceptedBy(loggerClass);
            }
        }

        {
            var filterInputParser = builder.Configuration.ReadTypeFromConfig<IFilterInputParser>("DynamicStrategies:IFilterInputParser");
            containerBuilder.RegisterType(filterInputParser)
                            .As<IFilterInputParser>()
                            .SingleInstance();
        }
    });

    // NFR: Reactive / Async — background services run on separate threads,
    // never blocking HTTP request handlers.
    builder.Services
        .AddBackgroundService(builder.Configuration, "DynamicStrategies:AlertEvaluationBackgroundService");

    builder.Services
        .AddHttpBackgroundService(builder.Configuration, "DynamicStrategies:AisFetcherService")
        .AddHttpBackgroundService(builder.Configuration, "DynamicStrategies:BerthOccupancyFetcherService")
        .AddHttpBackgroundService(builder.Configuration, "DynamicStrategies:CustomsDwellTimeFetcherService")
        .AddHttpBackgroundService(builder.Configuration, "DynamicStrategies:VesselDelayRateFetcherService")
        .AddHttpBackgroundService(builder.Configuration, "DynamicStrategies:WeatherFetcherService");

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

    builder.Services.AddSingletonFromConfig<IExceptionHandler>(builder.Configuration, "DynamicStrategies:GlobalExceptionHandler");
    builder.Services.AddProblemDetails();

    // NFR: Cross-cutting / Interceptors — BusinessLogicAuditFilter logs every
    // controller action (class, method, user, timestamp, duration).
    // Toggle via appsettings.json "Auditing:Enabled" — no recompile needed.
    builder.Services.AddControllers(options =>
    {
        var auditingEnabled = builder.Configuration.GetValue<bool>("Auditing:Enabled");
        if (auditingEnabled)
        {
            var auditFilter = builder.Configuration.ReadTypeFromConfig("DynamicStrategies:AuditFilter");
            options.Filters.Add(auditFilter);
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

    app.UseRouting();
    app.UseSession();

    app.Use(async (context, next) =>
    {
        if (string.IsNullOrEmpty(context.Session.GetString("SessionInit")))
        {
            context.Session.SetString("SessionInit", "True");
        }
        await next();
    });

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
