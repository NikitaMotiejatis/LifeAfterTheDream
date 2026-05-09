using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IRiskMonitorRepository
{
    ICollection<Kri> GetAllIndicators();
    ICollection<KriReading> GetAllReadings();
    ICollection<Alert> GetAllAlerts();
}
