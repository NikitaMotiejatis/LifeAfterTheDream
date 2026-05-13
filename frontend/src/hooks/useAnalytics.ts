import { useQuery } from '@tanstack/react-query';
import { fetchMockAnalyticsData } from '../mocks/analytics';

export interface ForecastItem {
  title: string;
  status: 'Low Risk' | 'Medium Risk' | 'High Risk' | 'Improving' | 'Stable' | 'Worsening';
  description: string;
  statusColor: 'green' | 'yellow' | 'red' | 'blue';
}

export interface AnalyticsData {
  berthOccupancy: any[];
  vesselDelayRate: any[];
  customsDwellTime: any[];
  weatherRisk: any[];
  disruptionIndex: any[];
  forecastSummary: ForecastItem[];
}


const USE_MOCK_DATA = true;


const fetchRealAnalyticsData = async (): Promise<AnalyticsData> => {
  // TODO: Replace with actual API call
  throw new Error('Real API not implemented yet');
};

const fetchAnalyticsData = async (): Promise<AnalyticsData> => {
  if (USE_MOCK_DATA) {
    return fetchMockAnalyticsData();
  }
  return fetchRealAnalyticsData();
};

export function useAnalytics() {
  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['analytics'],
    queryFn: fetchAnalyticsData,
  });

  return { data, isLoading, isError, refetch };
}