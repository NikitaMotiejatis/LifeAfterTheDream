namespace PortRiskMonitor.Application.Interfaces;

public interface IWeatherConditionService : IIndicatorScore
{
    ushort GetWindSpeedKnt();
    float GetWaterLevelCm();
    sbyte  GetTemperatureC();
    byte   GetHumidityPercent();
    string GetConditionCode();
}
