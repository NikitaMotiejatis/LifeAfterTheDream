using PortRiskMonitor.Infrastructure.Data;
using PortRiskMonitor.Infrastructure.Entities;

namespace PortRiskMonitor.Infrastructure.Repositories;

public interface IAuditLogRepository
{
    Task WriteAsync(AuditLog log);
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task WriteAsync(AuditLog log)
    {
        log.Id = Guid.NewGuid();
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
