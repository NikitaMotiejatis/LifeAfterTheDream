using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface INotificationService 
{
    Task<NotifyResponse> NotifyAsync(NotifyRequest req, CancellationToken ct);
}