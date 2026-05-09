export interface PortStatusDto {
  disruptionIndex: number;
  riskLevel: string;
  sparkline: number[];
}

export interface WeatherDto {
  windSpeedKts: number;
  waveHeightM: number;
  temperatureC: number;
  humidityPercent: number;
  description: string;
}

export interface ThresholdDto {
  label: string;
  color: string;
  severity: 'Low' | 'Medium' | 'High';
}

export interface KriCardDto {
  id: string;
  title: string;
  value: string;
  formula: string;
  thresholds: ThresholdDto[];
  sparkline: number[];
}

export interface ActiveVesselDto {
  name: string;
  berth: string;
  status: 'on-time' | 'delayed' | 'arrived';
}

export interface TrendPointDto {
  hour: string;
  value: number;
}

export interface VesselScheduleDto {
  name: string;
  imo: string;
  type: string;
  fuelType: string;
  cargo: string;
  berth: string;
  eta: string;
  status: 'On Time' | 'Delayed' | 'Arrived';
}

export interface DashboardData {
  portStatus: PortStatusDto;
  weather: WeatherDto;
  kriCards: KriCardDto[];
  activeVessels: ActiveVesselDto[];
  trendData: TrendPointDto[];
  vesselSchedule: VesselScheduleDto[];
}
