namespace RiskMonitor.Logic;

public interface IKriScore<T>
{
    public T GetScoreValue();
    public ICollection<(DateTime, T)> GetScores(DateTime? from, DateTime? to);
}
