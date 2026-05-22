import { AlertTriangle, Anchor, Clock, Cloud, TrendingUp } from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import AnalyticsCard from '../components/analytics/AnalyticsCard';
import AnalyticsHeader from '../components/analytics/AnalyticsHeader';
import ForecastSummaryCard from '../components/analytics/ForecastSummaryCard';
import ErrorCard from '../components/common/ErrorCard';
import Spinner from '../components/common/Spinner';
import { useAnalytics } from '../hooks/useAnalytics';
import { fetchMockAnalyticsDataWithRange } from '../mocks/analytics';
import type { DateTimeRange, TrendDataPoint } from '../types/AnalyticsIndex';

function computeMetricYDomain(
  data: TrendDataPoint[],
  greenThreshold: number,
  yellowThreshold: number,
  fallbackMax = 20,
): [number, number] {
  const values = data
    .flatMap((d) => [d.historical, d.forecast])
    .filter((v): v is number => v !== null);

  if (values.length === 0) return [0, fallbackMax];

  const dataMin = Math.min(...values);
  const dataMax = Math.max(...values);

  const min = Math.floor(Math.min(dataMin, greenThreshold) * 0.9);
  const max = Math.ceil(Math.max(dataMax, yellowThreshold) * 1.1);

  return [Math.max(0, min), max];
}

export default function AnalyticsPage() {
  const { data, isLoading, isError, refetch } = useAnalytics();

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
        'port-disruption': data.disruptionIndex,
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
        if (metricId === 'port-disruption')
          targetData = newData.disruptionIndex;

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

  if (isLoading) return <Spinner />;
  if (isError || Object.keys(cardsData).length === 0)
    return (
      <ErrorCard
        message="Failed to load analytics data."
        onRetry={() => refetch()}
      />
    );

  const trendMetrics = [
    {
      id: 'berth-occupancy',
      icon: Anchor,
      title: 'Berth Occupancy',
      description: 'Port capacity utilization trend',
      data: cardsData['berth-occupancy'] || [],
      yAxisLabel: 'Occupancy (%)',
      greenThreshold: 50,
      yellowThreshold: 75,
      yAxisDomain: [0, 100] as [number, number],
    },
    {
      id: 'vessel-delay-rate',
      icon: AlertTriangle,
      title: 'Vessel Delay Rate',
      description: 'Percentage of delayed arrivals',
      data: cardsData['vessel-delay-rate'] || [],
      yAxisLabel: 'Delay Rate (%)',
      greenThreshold: 10,
      yellowThreshold: 25,
      yAxisDomain: [0, 100] as [number, number],
    },
    {
      id: 'customs-dwell-time',
      icon: Clock,
      title: 'Customs Dwell Time',
      description: 'Average container clearance time',
      data: cardsData['customs-dwell-time'] || [],
      yAxisLabel: 'Dwell Time (hours)',
      greenThreshold: 5,
      yellowThreshold: 12,
      yAxisDomain: computeMetricYDomain(
        cardsData['customs-dwell-time'] || [],
        5,
        12,
        20,
      ),
    },
    {
      id: 'weather-risk',
      icon: Cloud,
      title: 'Weather Risk Score',
      description: 'Environmental risk assessment',
      data: cardsData['weather-risk'] || [],
      yAxisLabel: 'Risk Score',
      greenThreshold: 30,
      yellowThreshold: 70,
      yAxisDomain: [0, 100] as [number, number],
    },
    {
      id: 'port-disruption',
      icon: TrendingUp,
      title: 'Port Disruption Index',
      description: 'Overall operational disruption level',
      data: cardsData['port-disruption'] || [],
      yAxisLabel: 'Disruption Index (%)',
      greenThreshold: 30,
      yellowThreshold: 60,
      yAxisDomain: [0, 100] as [number, number],
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
            icon={metric.icon}
            title={metric.title}
            description={metric.description}
            data={metric.data}
            yAxisLabel={metric.yAxisLabel}
            greenThreshold={metric.greenThreshold}
            yellowThreshold={metric.yellowThreshold}
            yAxisDomain={metric.yAxisDomain}
            onFilterApply={(range) => handleFilterApply(metric.id, range)}
            isLoading={loadingCardIds[metric.id] || false}
          />
        ))}
      </div>

      {data?.forecastSummary && (
        <ForecastSummaryCard data={data.forecastSummary} />
      )}
    </div>
  );
}
