import type {
  DashboardData,
  DateRange,
  TimeFrame,
  TrendPointDto,
} from '../types/Dashboard';
import { getMockTiles, getMockTrend } from '../mocks/dashboardMock';
import axiosInstance from './axiosInstance';

const USE_MOCK = false;

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

  const to = new Date();
  const from = new Date(to);
  const offsets: Record<string, number> = { '6h': 6, '12h': 12, '24h': 24, '48h': 48, '72h': 72, week: 168, month: 720, year: 8760 };
  from.setHours(from.getHours() - (offsets[dateRange.preset] ?? 24));
  const totalHours = (to.getTime() - from.getTime()) / (1000 * 60 * 60);
  const take = Math.min(500, Math.max(20, Math.floor(totalHours / 24) * 20)); // ~20 points per day, max 500

  const cards = await axiosInstance
    .get('/history/cards', { params: { from: from.toISOString(), to: to.toISOString(), take } })
    .then((r) => r.data);


  const mock = getMockTiles(dateRange);
  const kriCards = cards.map((card: any) => ({
	...card,
	sparkline: card.sparkline.map((pt: { timestamp: string; value: number }) => ({
		label: formatLabel(new Date(pt.timestamp), totalHours),
		value: pt.value,
	})),
	}));
	return { ...mock, kriCards };
}

export async function fetchTrendData(
  trendTimeFrame: TimeFrame,
): Promise<TrendPointDto[]> {
  if (true) {
    await new Promise((r) => setTimeout(r, 300));
    return getMockTrend(trendTimeFrame);
  }

  // TODO: axiosInstance.get('/dashboard/trend', { params: { trendTimeFrame } }).then((r) => r.data);
  throw new Error('Backend not configured.');
}

function formatLabel(d: Date, totalHours: number): string {
  if (totalHours <= 48)
    return d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
  if (totalHours <= 168)
    return d.toLocaleDateString('en-GB', { weekday: 'short', day: '2-digit', month: 'short' });
  return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
}
