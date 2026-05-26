# Port Risk Monitor — Backend

.NET 10 Web API for real-time port operations risk monitoring.

## Architecture (3-tier multi-layer)

```
┌─────────────────────────────────────────────────────────┐
│  Presentation Layer     React 18 + Vite (frontend/)     │
│                         Tailwind CSS + Recharts         │
└──────────────────────────┬──────────────────────────────┘
                           │ HTTP / REST
┌──────────────────────────▼──────────────────────────────┐
│  Business Logic Layer   ASP.NET Core 10 Web API         │
│                         PortRiskMonitor.API             │
│                         PortRiskMonitor.Application     │
│                         RiskMonitor (domain library)    │
└──────────────────────────┬──────────────────────────────┘
                           │ EF Core
┌──────────────────────────▼──────────────────────────────┐
│  Data Access Layer      Entity Framework Core 10        │
│                         PortRiskMonitor.Infrastructure  │
│                         SQLite                          │
└─────────────────────────────────────────────────────────┘
```

## Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet):
    ```bash
    dotnet tool install --global dotnet-ef
    ```

### Database

The project uses **SQLite** (file-based, zero external dependencies). The database file `port_risk_monitor.db` is created automatically in `PortRiskMonitor.API/` on first run.

**Automatic setup (default):** EF migrations and seed data are applied on every startup in `Program.cs`. No manual steps needed.

**Manual migration commands** (if you need to reset or re-create):

```bash
cd PortRiskMonitor.API

# Apply all migrations (creates/updates the DB schema)
dotnet ef database update --project ../PortRiskMonitor.Infrastructure

# Reset the database (delete and recreate from scratch)
del port_risk_monitor.db
dotnet ef database update --project ../PortRiskMonitor.Infrastructure

# Add a new migration after entity changes
dotnet ef migrations add <MigrationName> --project ../PortRiskMonitor.Infrastructure
```

The connection string is configured in `appsettings.json` under `ConnectionStrings:DefaultConnection`.

### Run

```bash
cd PortRiskMonitor.API
dotnet restore ../../PortRiskMonitor.sln
dotnet run
```

Swagger UI available at the root URL (Development mode). Seed data (5 KRI definitions + 1 year of hourly readings) is inserted if the DB is empty.

## Project Structure

```
backend/
├── PortRiskMonitor.sln
├── PortRiskMonitor.API/                ← Presentation Layer
│   ├── Controllers/
│   │   ├── DashboardController.cs
│   │   ├── AnalyticsController.cs
│   │   └── SettingsController.cs
│   ├── Filters/
│   │   └── BusinessLogicAuditFilter.cs    ← NFR: Interceptors
│   ├── Exceptions/
│   │   └── GlobalExceptionHandler.cs
│   ├── Program.cs                         ← DI wiring, middleware pipeline
│   └── appsettings.json                   ← Toggle flags (auditing, strategy)
├── PortRiskMonitor.Application/        ← Business Logic Layer
│   ├── Interfaces/                        ← Service contracts
│   ├── Services/
│   │   ├── DashboardService.cs
│   │   ├── AnalyticsService.cs
│   │   ├── ThresholdSettingsService.cs
│   │   ├── AlertingService.cs
│   │   └── FilterInputParser.cs
│   ├── BackgroundServices/
│   │   ├── AlertEvaluationBackgroundService.cs  ← NFR: Async
│   │   ├── WeatherFetcherService.cs
│   │   └── AisFetcherService.cs
│   └── DTOs/
├── PortRiskMonitor.Infrastructure/     ← Data Access Layer
│   ├── Data/
│   │   ├── AppDbContext.cs                ← NFR: Optimistic Locking (RowVersion)
│   │   └── SeedData.cs
│   ├── Entities/
│   │   ├── KriDefinition.cs
│   │   └── AuditLog.cs
│   ├── Repositories/
│   │   ├── KriRepository.cs              ← NFR: Security (EF Core LINQ only)
│   │   └── AuditLogRepository.cs
│   ├── Alerts/
│   │   └── AlertRepository.cs
│   ├── Notifications/
│   │   ├── AwsSnsAlertNotifier.cs         ← NFR: Extensibility (Strategy)
│   │   └── NullAlertNotifier.cs           ← NFR: Extensibility (Strategy)
│   └── PortStatus/
└── RiskMonitor/                        ← Domain library
    ├── Entities/ (Kri, KriReading, Alert)
    ├── DTOs/ (ScoreInfo, RiskLevel, BucketType)
    ├── Logic/ (IRiskCalculator, IKriScore)
    ├── Repositories/ (IKriRepository, IRiskMonitorRepository)
    ├── Services/ (IAlertNotifier)
    └── Extensions/ (ScoreInfoExtensions — M4 downsampling)
```

## Non-Functional Requirements

| NFR                              | Implementation                                                                                                                                                                                                                | File & Line                                                                                                         |
| -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| **Concurrency**                  | All services are Scoped (per-request). No session state. Multiple browser tabs use the same account without conflicts.                                                                                                        | `Program.cs` — `AddScoped<>` registrations                                                                          |
| **Security (SQL injection)**     | All DB queries use EF Core LINQ — no raw SQL, no string interpolation in queries.                                                                                                                                             | `KriRepository.cs`, `AlertRepository.cs`, `PortRiskMonitorRepo.cs`                                                  |
| **Data Access (ORM + tx scope)** | Entity Framework Core ORM. `SaveChangesAsync()` is called within a single method per HTTP request — transaction never spans user interaction.                                                                                 | `KriRepository.cs`, `AuditLogRepository.cs`, `AlertRepository.cs`                                                   |
| **Optimistic Locking**           | `Kri.RowVersion` with `[Timestamp]` attribute. EF Core throws `DbUpdateConcurrencyException` on conflict. Frontend intercepts 409 responses.                                                                                  | `Kri.cs` — `RowVersion` property; `AppDbContext.cs` — `.IsConcurrencyToken()`; `axiosInstance.ts` — 409 interceptor |
| **Memory Management**            | Services registered as Scoped (new instance per request). Only stateless caches (`WeatherSnapshotCache`, `AisSnapshotCache`) are Singleton with `volatile` field.                                                             | `Program.cs`                                                                                                        |
| **Async / Non-blocking**         | All controller actions are `async Task`. Background services (`AlertEvaluationBackgroundService`, `WeatherFetcherService`, `AisFetcherService`) run on separate threads. Frontend uses React Query with auto-refetch.         | `AlertEvaluationBackgroundService.cs`, `WeatherFetcherService.cs`                                                   |
| **Cross-cutting / Interceptors** | `BusinessLogicAuditFilter` (global `IAsyncActionFilter`) logs every action: class, method, user, permissions, timestamp, duration, outcome. Toggle via `appsettings.json` `"Auditing:Enabled"` — no code modification needed. | `BusinessLogicAuditFilter.cs`, `appsettings.json`                                                                   |
| **Extensibility / Strategy**     | `IAlertNotifier` interface with implementations `AwsSnsAlertNotifier` and `NullAlertNotifier`. Selected via config (`Alerts:Sms:Enabled`). New notifiers can be added without modifying existing code.                        | `IAlertNotifier.cs`, `AwsSnsAlertNotifier.cs`, `NullAlertNotifier.cs`, `Program.cs`                                 |
