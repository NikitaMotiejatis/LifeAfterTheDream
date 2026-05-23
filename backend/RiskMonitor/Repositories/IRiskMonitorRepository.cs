using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IRiskMonitorRepository
{
    IQueryable<Kri> GetAllIndicators();
    IQueryable<KriReading> GetAllReadings();
    IQueryable<Alert> GetAllAlerts();

    IQueryable<KriReading> GetKriReadings(string slug);
}
