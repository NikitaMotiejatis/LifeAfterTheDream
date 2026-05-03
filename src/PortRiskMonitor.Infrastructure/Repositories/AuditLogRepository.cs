// ============================================================
// AuditLogRepository.cs — Write-only repository for audit records
//
// AuditLogs are append-only. They are never modified after creation.
// This repository is used exclusively by BusinessLogicAuditFilter.
// ============================================================

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

        // TODO: Consider using a fire-and-forget pattern here (no await) so audit
        //       logging never delays the actual HTTP response. Trade-off: you might
        //       lose the audit log entry if the process crashes between the response
        //       and the DB write. For a PoC, awaiting is fine.
        await _context.SaveChangesAsync();
    }
}
