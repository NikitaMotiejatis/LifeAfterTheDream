import { AlertTriangle, Anchor, Clock, Cloud, TrendingUp } from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import AnalyticsCard from '../components/analytics/AnalyticsCard';
import AnalyticsHeader from '../components/analytics/AnalyticsHeader';
import ForecastSummaryCard from '../components/analytics/ForecastSummaryCard';
import { useAnalytics } from '../hooks/useAnalytics';
import { fetchMockAnalyticsDataWithRange } from '../mocks/analytics';
import type { DateTimeRange, TrendDataPoint } from '../types/AnalyticsIndex';

export default function AnalyticsPage() {
  const { data } = useAnalytics();

  const [cardsData, setCardsData] = useState<Record<string, TrendDataPoint[]>>(
    {},
  );
  const [loadingCardIds, setLoadingCardIds] = useState<Record<string, boolean>>(
    {},
  );

  useEffect(() => {
    if (data) {
      setCardsData({
        'berth-occupancy': data.berthOccupancy,
        'vessel-delay-rate': data.vesselDelayRate,
        'customs-dwell-time': data.customsDwellTime,
        'weather-risk': data.weatherRisk,
        'port-status': data.disruptionIndex,
      });
    }
  }, [data]);

  const handleFilterApply = useCallback(
    async (metricId: string, range: DateTimeRange) => {
      setLoadingCardIds((prev) => ({ ...prev, [metricId]: true }));
      try {
        console.log(
          `Filtering ${metricId} from ${range.fromDate}T${range.fromTime} to ${range.toDate}T${range.toTime}`,
        );

        const newData = await fetchMockAnalyticsDataWithRange(range);

        let targetData: TrendDataPoint[] = [];
        if (metricId === 'berth-occupancy') targetData = newData.berthOccupancy;
        if (metricId === 'vessel-delay-rate')
          targetData = newData.vesselDelayRate;
        if (metricId === 'customs-dwell-time')
          targetData = newData.customsDwellTime;
        if (metricId === 'weather-risk') targetData = newData.weatherRisk;
        if (metricId === 'port-status') targetData = newData.disruptionIndex;

        setCardsData((prev) => ({
          ...prev,
          [metricId]: targetData,
        }));
      } catch (error) {
        console.error('Filter failed:', error);
      } finally {
        setLoadingCardIds((prev) => ({ ...prev, [metricId]: false }));
      }
    },
    [],
  );

  const trendMetrics = [
    {
      id: 'berth-occupancy',
      icon: Anchor,
      description: 'Port capacity utilization trend',
      yAxisLabel: 'Occupancy (%)',
      ymin: 0,
      ymax: 100,
    },
    {
      id: 'vessel-delay-rate',
      icon: AlertTriangle,
      description: 'Percentage of delayed arrivals',
      yAxisLabel: 'Delay Rate (%)',
      ymin: 0,
      ymax: 100,
    },
    {
      id: 'customs-dwell-time',
      icon: Clock,
      description: 'Average container clearance time',
      yAxisLabel: 'Dwell Time (hours)',
      ymin: 0,
      ymax: null,
    },
    {
      id: 'weather-risk',
      icon: Cloud,
      description: 'Environmental risk assessment',
      yAxisLabel: 'Risk Score',
      ymin: 0,
      ymax: 100,
    },
    {
      id: 'port-status',
      icon: TrendingUp,
      description: 'Overall operational disruption level',
      yAxisLabel: 'Disruption Index (%)',
      ymin: 0,
      ymax: 100,
    },
  ];

  return (
    <div className="space-y-6">
      <AnalyticsHeader />

      {/* Trends Grid */}
      <div className="grid grid-cols-1 gap-6">
        {trendMetrics.map((metric) => (
          <AnalyticsCard
            key={metric.id}
            id={metric.id}
            icon={metric.icon}
            description={metric.description}
            yAxisLabel={metric.yAxisLabel}
            ymin={metric.ymin}
            ymax={metric.ymax}
            onFilterApply={(range) => handleFilterApply(metric.id, range)}
          />
        ))}
      </div>

      {data?.forecastSummary && (
        <ForecastSummaryCard data={data.forecastSummary} />
      )}
    </div>
  );
}
