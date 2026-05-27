export interface ThresholdPair {
  green: number;
  yellow: number;
  xmin?: number;
}

export interface WeatherRiskSettings extends ThresholdPair {
  waveHeightMultiplier: number;
}

export interface DisruptionIndexSettings extends ThresholdPair {
  berthWeight: number;
  delayWeight: number;
  customsWeight: number;
  weatherWeight: number;
}

export interface FormulaSettings {
  berthOccupancy: ThresholdPair;
  vesselDelayRate: ThresholdPair;
  customsDwellTime: ThresholdPair;
  weatherRisk: WeatherRiskSettings;
  disruptionIndex: DisruptionIndexSettings;
}
