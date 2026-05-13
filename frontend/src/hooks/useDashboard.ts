import { useQuery } from '@tanstack/react-query';
import { fetchDashboardData } from '../api/dashboardApi';

export function useDashboard() {
  return useQuery({
    queryKey: ['dashboard'],
    queryFn: fetchDashboardData,
    refetchInterval: 30_000, // auto-refresh every 30s
    staleTime: 10_000,
  });
}
