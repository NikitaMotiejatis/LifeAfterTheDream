export type TimeFrame = '24h' | '7d' | '30d' | '90d' | '6m' | '1y';

export type DashboardRange =
  | '6h'
  | '12h'
  | '24h'
  | '48h'
  | '72h'
  | 'week'
  | 'month'
  | 'year'
  | 'custom';

export interface DateRange {
  preset: DashboardRange;
  from: string | null; // ISO string, used only when preset === 'custom'
  to: string | null;
}

export interface SparkPoint {
  label: string;
  value: number;
}

export interface PortStatusDto {
  disruptionIndex: number;
  riskLevel: string;
  greenMax: number;
  yellowMax: number;
  sparkline: SparkPoint[];
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
  severity: 'Low' | 'Medium' | 'High';
  greenMax: number;
  yellowMax: number;
  sparkline: SparkPoint[];
}

export interface ActiveVesselDto {
  name: string;
  berth: string;
  status: 'on-time' | 'delayed' | 'arrived';
}

export interface TrendPointDto {
  label: string;
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
  periodLabel: string;
  portStatus: PortStatusDto;
  weather: WeatherDto;
  kriCards: KriCardDto[];
  activeVessels: ActiveVesselDto[];
  trendData: TrendPointDto[];
  vesselSchedule: VesselScheduleDto[];
}
