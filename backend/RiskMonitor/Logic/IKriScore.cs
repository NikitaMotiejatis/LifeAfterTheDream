namespace RiskMonitor.Logic;

public interface IKriScore<T>
{
    public T GetScoreValue();
    public ICollection<(DateTime Timestamp, T Score)> GetScores(DateTime? from, DateTime? to);
}
