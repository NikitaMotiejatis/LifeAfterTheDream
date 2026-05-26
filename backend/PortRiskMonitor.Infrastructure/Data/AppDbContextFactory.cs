using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PortRiskMonitor.Infrastructure.Data;

// Design-time factory for EF Core CLI tools (dotnet ef migrations).
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=port_risk_monitor_designtime.db");
        return new AppDbContext(optionsBuilder.Options);
    }
}
