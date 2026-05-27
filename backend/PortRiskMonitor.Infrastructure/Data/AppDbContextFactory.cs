using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace PortRiskMonitor.Infrastructure.Data;

// Design-time factory for EF Core CLI tools (dotnet ef migrations).
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=portrisks;Username=portuser;Password=portpass")
            .Options;

        return new AppDbContext(options);
    }
}
