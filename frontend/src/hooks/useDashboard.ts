import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { fetchDashboardTiles, fetchTrendData } from '../api/dashboardApi';
import type { DateRange, TimeFrame } from '../types/Dashboard';

const defaultRange: DateRange = { preset: '24h', from: null, to: null };

export function useDashboard(
  dateRange: DateRange = defaultRange,
  trendTimeFrame: TimeFrame = '24h',
) {
  const tiles = useQuery({
    queryKey: ['dashboard-tiles', dateRange],
    queryFn: () => fetchDashboardTiles(dateRange),
    refetchInterval: 30_000,
    staleTime: 10_000,
    placeholderData: keepPreviousData,
  });

  const trend = useQuery({
    queryKey: ['dashboard-trend', trendTimeFrame],
    queryFn: () => fetchTrendData(trendTimeFrame),
    refetchInterval: 30_000,
    staleTime: 10_000,
    placeholderData: keepPreviousData,
  });

  return {
    tiles,
    trend,
    isLoading: tiles.isLoading || trend.isLoading,
    isError: tiles.isError || trend.isError,
    refetch: () => {
      tiles.refetch();
      trend.refetch();
    },
  };
}
