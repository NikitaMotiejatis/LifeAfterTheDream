// ============================================================
// AppDbContextFactory.cs — Design-time DbContext factory
//
// WHY THIS FILE EXISTS:
//   When you run "dotnet ef migrations add ...", EF Core tools
//   need to create an instance of AppDbContext without running
//   the full ASP.NET Core host (Program.cs is not executed).
//
//   Without this file, EF Core tries to start the API project
//   to find the DbContext — which often fails with errors like:
//     "Unable to create an object of type 'AppDbContext'"
//     "No database provider has been configured"
//
//   This factory gives EF Core a direct, standalone way to
//   create AppDbContext using just a hardcoded SQLite connection.
//   It is ONLY used by EF Core CLI tools — never at runtime.
//
// USAGE:
//   From the solution root, run:
//     dotnet ef migrations add InitialCreate --project src/PortRiskMonitor.Infrastructure
//   No --startup-project flag needed when this factory is present.
// ============================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace PortRiskMonitor.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=portrisks;Username=portuser;Password=portpass")
            .Options;

        return new AppDbContext(options);
    }
}
