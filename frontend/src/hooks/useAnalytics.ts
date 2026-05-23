import { useCallback, useEffect, useState } from 'react';
import { fetchMockAnalyticsData } from '../mocks/analytics';
import type { AnalyticsData, DateTimeRange } from '../types/AnalyticsIndex';

export type {
  AnalyticsData,
  ForecastItem,
  TrendDataPoint,
} from '../types/AnalyticsIndex';

export function useAnalytics(initialRange?: DateTimeRange) {
  const [data, setData] = useState<AnalyticsData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isError, setIsError] = useState(false);
  const [activeRange, setActiveRange] = useState<DateTimeRange | undefined>(
    initialRange,
  );

  const fetchData = useCallback(async (range?: DateTimeRange) => {
    setIsLoading(true);
    setIsError(false);
    try {
      const result = await fetchMockAnalyticsData();
      setData(result);
      if (range) setActiveRange(range);
    } catch (error) {
      setIsError(true);
      console.error('Failed to fetch analytics:', error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const refetch = useCallback(
    (range?: DateTimeRange) => fetchData(range || activeRange),
    [fetchData, activeRange],
  );

  useEffect(() => {
    fetchData(activeRange);
  }, []);

  return { data, isLoading, isError, refetch, activeRange };
}
