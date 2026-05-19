import type {
  DashboardData,
  DateRange,
  KriCardDto,
  PortStatusDto,
  TimeFrame,
  TrendPointDto,
  WeatherDto,
} from '../types/Dashboard';
import { getMockTiles } from '../mocks/dashboardMock';
import axiosInstance from './axiosInstance';

export async function fetchDashboardTiles(
  dateRange: DateRange,
): Promise<DashboardData> {
  const params = {
    preset: dateRange.preset,
    from: dateRange.from,
    to: dateRange.to,
  };

  const dashboardTiles = getMockTiles(dateRange);

  ((dashboardTiles.portStatus = await axiosInstance
    .get('/dashboard/port-status', { params })
    .then((r) => r.data as PortStatusDto)),
    (dashboardTiles.weather = await axiosInstance
      .get('/dashboard/weather')
      .then((r) => r.data as WeatherDto)));

  dashboardTiles.kriCards = await axiosInstance
    .get('/dashboard/kri-cards', { params })
    .then((r) => r.data as KriCardDto[]);

  return dashboardTiles;
}

export async function fetchTrendData(
  trendTimeFrame: TimeFrame,
): Promise<TrendPointDto[]> {
  return await axiosInstance
    .get('/dashboard/trend', { params: { trendTimeFrame } })
    .then((r) => r.data as TrendPointDto[]);
}
