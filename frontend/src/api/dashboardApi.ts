import type { DashboardData } from '../types/Dashboard';
import { mockDashboardData } from '../mocks/dashboardMock';
// import axiosInstance from './axiosInstance';

const USE_MOCK = true;

export async function fetchDashboardData(): Promise<DashboardData> {
  if (USE_MOCK) {
    // Simulate network delay
    await new Promise((r) => setTimeout(r, 600));
    return mockDashboardData;
  }

  // TODO: Wire up to real C# backend endpoints
  // const [portStatus, weather, kriCards, activeVessels, trendData, vesselSchedule] =
  //   await Promise.all([
  //     axiosInstance.get('/dashboard/port-status').then((r) => r.data),
  //     axiosInstance.get('/dashboard/weather').then((r) => r.data),
  //     axiosInstance.get('/dashboard/kri-cards').then((r) => r.data),
  //     axiosInstance.get('/dashboard/active-vessels').then((r) => r.data),
  //     axiosInstance.get('/dashboard/trend').then((r) => r.data),
  //     axiosInstance.get('/dashboard/vessel-schedule').then((r) => r.data),
  //   ]);
  // return { portStatus, weather, kriCards, activeVessels, trendData, vesselSchedule };

  throw new Error(
    'Backend not configured. Set USE_MOCK = true or implement API calls.',
  );
}
