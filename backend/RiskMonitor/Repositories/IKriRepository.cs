using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IKriRepository
{
    ICollection<KriReading> GetAllReadings();
    ICollection<Alert> GetAllAlerts();
}
