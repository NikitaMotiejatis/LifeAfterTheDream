# 🚢 Port / Cargo Operations Risk Monitor

> Internal Proof of Concept — .NET 8 + React 18

A real-time risk monitoring dashboard for port operations, built on a 3-tier architecture using ASP.NET Core, Entity Framework Core, and React.

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│  Presentation Layer     React 18 + Vite (frontend/)      │
│                         Tailwind CSS + Recharts           │
└──────────────────────────┬──────────────────────────────┘
                           │ HTTP / REST
┌──────────────────────────▼──────────────────────────────┐
│  Business Logic Layer   ASP.NET Core 8 Web API           │
│                         PortRiskMonitor.API               │
│                         PortRiskMonitor.Application       │
└──────────────────────────┬──────────────────────────────┘
                           │ EF Core
┌──────────────────────────▼──────────────────────────────┐
│  Data Access Layer      Entity Framework Core 8          │
│                         PortRiskMonitor.Infrastructure    │
│                         SQLite (PoC) / SQL Server (Prod) │
└─────────────────────────────────────────────────────────┘
```

---

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### Backend setup

```bash
# From the repo root
cd src/PortRiskMonitor.API

# Restore packages
dotnet restore ../../PortRiskMonitor.sln

# Apply database migrations (creates port_risk_monitor.db)
dotnet ef database update --project ../PortRiskMonitor.Infrastructure

# Run the API (starts on https://localhost:5001)
dotnet run
```

Swagger UI is available at: **https://localhost:5001** (in Development mode)

### Frontend setup

```bash
cd frontend
npm install
npm run dev   # starts on http://localhost:5173
```

---

## Non-Functional Requirements — Quick Reference

| NFR | Where to find it |
|-----|-----------------|
| Concurrency (no session state) | `Program.cs` — all services `AddScoped`, no `AddSingleton` for stateful objects |
| Security (SQL injection) | `KriRepository.cs` — all queries use EF Core LINQ |
| Data Access (ORM + tx scope) | `KriRepository.cs` — `SaveChangesAsync` called once per method |
| Optimistic Locking | `KriDefinition.cs` — `[Timestamp] RowVersion` property; `KrisController.cs` — catches `DbUpdateConcurrencyException` → returns 409 |
| Memory Management | `Program.cs` — `AddScoped<>` for all services |
| Async / Non-blocking | All controllers and services use `async/await`; `MockDataBackgroundService` runs on background thread |
| Interceptors / Audit Logging | `BusinessLogicAuditFilter.cs`; toggle via `appsettings.json` `Auditing:Enabled` |
| Extensibility / Strategy | `RiskScoreEngine.cs` + `DefaultWeightedStrategy.cs`; swap via `appsettings.json` `RiskScoring:Strategy` |

---

## Configuration

Key settings in `src/PortRiskMonitor.API/appsettings.json`:

```json
{
  "Auditing": { "Enabled": true },
  "RiskScoring": { "Strategy": "DefaultWeighted" },
  "MockData": {
    "Enabled": true,
    "IntervalSeconds": 30,
    "DefaultScenario": "Normal"
  }
}
```

## Demo Scenarios (Development only)

Use the Demo Control Panel in the React UI, or call directly:

```bash
curl -X POST https://localhost:5001/api/scenarios/activate \
  -H "Content-Type: application/json" \
  -d '{"scenarioName": "StormEvent"}'
```

Available: `Normal` | `MildCongestion` | `StormEvent` | `CustomsCrisis` | `FullRedAlert`

---

## Project Structure

```
PortRiskMonitor/
├── PortRiskMonitor.sln
├── README.md
└── src/
    ├── PortRiskMonitor.API/               ← Presentation Layer
    │   ├── Controllers/
    │   │   ├── KrisController.cs          ← CRUD + dashboard polling
    │   │   └── OtherControllers.cs        ← Alerts, Reports, Scenarios
    │   ├── Filters/
    │   │   └── BusinessLogicAuditFilter.cs ← NFR: Interceptors
    │   ├── Program.cs                     ← DI wiring, middleware pipeline
    │   └── appsettings.json               ← All toggle flags
    ├── PortRiskMonitor.Application/       ← Business Logic Layer
    │   ├── DTOs/
    │   │   └── Dtos.cs                    ← API contract (mirror in React types)
    │   ├── Interfaces/
    │   │   └── IServiceInterfaces.cs      ← Service contracts
    │   └── Services/
    │       ├── KriService.cs              ← Main CRUD + dashboard logic
    │       ├── RiskScoreEngine.cs         ← NFR: Strategy Pattern
    │       ├── AlertAndMockServices.cs    ← AlertService + MockDataBackgroundService
    │       └── ReportService.cs           ← Report generation
    └── PortRiskMonitor.Infrastructure/    ← Data Access Layer
        ├── Data/
        │   └── AppDbContext.cs            ← EF Core DbContext
        ├── Entities/
        │   ├── KriDefinition.cs           ← NFR: Optimistic locking (RowVersion)
        │   ├── KriReading.cs              ← Time-series readings
        │   ├── Alert.cs                   ← Threshold breach events
        │   └── AuditLog.cs               ← NFR: Audit trail records
        └── Repositories/
            ├── IKriRepository.cs          ← NFR: Security (all parameterized)
            ├── KriRepository.cs           ← NFR: Data Access (tx per request)
            ├── AlertRepository.cs
            └── AuditLogRepository.cs
```

---

## Team

| Person | Owns |
|--------|------|
| Person A (this repo) | .NET backend skeleton — all files in `src/` |
| Person B | React frontend — all files in `frontend/` |

**Sync point:** Align TypeScript interfaces in `frontend/src/types/` with DTOs in `Application/DTOs/Dtos.cs` — field names must match (camelCase on frontend, PascalCase on backend — ASP.NET Core serializes automatically).
