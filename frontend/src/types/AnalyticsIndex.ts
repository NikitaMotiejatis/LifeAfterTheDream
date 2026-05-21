export interface TrendDataPoint {
  label: string;
  index: number;
  historical: number | null;
  forecast: number | null;
}

export type ForecastStatus =
  | 'Low Risk'
  | 'Medium Risk'
  | 'High Risk'
  | 'Improving'
  | 'Stable'
  | 'Worsening';

export interface ForecastItem {
  title: string;
  status: ForecastStatus;
  description: string;
  statusColor: 'green' | 'yellow' | 'red';
}

export interface AnalyticsData {
  berthOccupancy: TrendDataPoint[];
  vesselDelayRate: TrendDataPoint[];
  customsDwellTime: TrendDataPoint[];
  weatherRisk: TrendDataPoint[];
  disruptionIndex: TrendDataPoint[];
  forecastSummary: ForecastItem[];
}

export interface DateTimeRange {
  fromDate: string;
  fromTime: string;
  toDate: string;
  toTime: string;
}
