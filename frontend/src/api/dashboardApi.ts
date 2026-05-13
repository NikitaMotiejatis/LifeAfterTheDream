import type {
  DashboardData,
  DateRange,
  TimeFrame,
  TrendPointDto,
} from '../types/Dashboard';
import { getMockTiles, getMockTrend } from '../mocks/dashboardMock';
// import axiosInstance from './axiosInstance';

const USE_MOCK = true;

export async function fetchDashboardTiles(
  dateRange: DateRange,
): Promise<DashboardData> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    return getMockTiles(dateRange);
  }

  // TODO: Wire up to real C# backend endpoints
  // const params = { preset: dateRange.preset, from: dateRange.from, to: dateRange.to };
  // const [portStatus, weather, kriCards, activeVessels, vesselSchedule] =
  //   await Promise.all([
  //     axiosInstance.get('/dashboard/port-status', { params }).then((r) => r.data),
  //     axiosInstance.get('/dashboard/weather', { params }).then((r) => r.data),
  //     axiosInstance.get('/dashboard/kri-cards', { params }).then((r) => r.data),
  //     axiosInstance.get('/dashboard/active-vessels', { params }).then((r) => r.data),
  //     axiosInstance.get('/dashboard/vessel-schedule', { params }).then((r) => r.data),
  //   ]);
  // return { periodLabel: dateRange.preset, portStatus, weather, kriCards, activeVessels, trendData: [], vesselSchedule };

  throw new Error('Backend not configured.');
}

export async function fetchTrendData(
  trendTimeFrame: TimeFrame,
): Promise<TrendPointDto[]> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 300));
    return getMockTrend(trendTimeFrame);
  }

  // TODO: axiosInstance.get('/dashboard/trend', { params: { trendTimeFrame } }).then((r) => r.data);
  throw new Error('Backend not configured.');
}
