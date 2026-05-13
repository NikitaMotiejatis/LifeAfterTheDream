import { useAnalytics } from '../hooks/useAnalytics';
import Spinner from '../components/common/Spinner';
import ErrorCard from '../components/common/ErrorCard';
import AnalyticsCard from '../components/analytics/AnalyticsCard';
import AnalyticsHeader from '../components/analytics/AnalyticsHeader';
import ForecastSummaryCard from '../components/analytics/ForecastSummaryCard';
import { Anchor, AlertTriangle, Clock, Cloud, TrendingUp } from 'lucide-react';

export default function AnalyticsPage() {
  const { data, isLoading, isError, refetch } = useAnalytics();

  if (isLoading) return <Spinner />;
  if (isError || !data)
    return (
      <ErrorCard
        message="Failed to load analytics data."
        onRetry={() => refetch()}
      />
    );

  const trendMetrics = [
    {
      icon: Anchor,
      title: 'Berth Occupancy',
      description: 'Port capacity utilization trend',
      data: data.berthOccupancy,
      yAxisLabel: 'Occupancy (%)',
      greenThreshold: 50,
      yellowThreshold: 75,
    },
    {
      icon: AlertTriangle,
      title: 'Vessel Delay Rate',
      description: 'Percentage of delayed arrivals',
      data: data.vesselDelayRate,
      yAxisLabel: 'Delay Rate (%)',
      greenThreshold: 10,
      yellowThreshold: 25,
    },
    {
      icon: Clock,
      title: 'Customs Dwell Time',
      description: 'Average container clearance time',
      data: data.customsDwellTime,
      yAxisLabel: 'Dwell Time (hours)',
      greenThreshold: 5,
      yellowThreshold: 12,
    },
    {
      icon: Cloud,
      title: 'Weather Risk Score',
      description: 'Environmental risk assessment',
      data: data.weatherRisk,
      yAxisLabel: 'Risk Score (0-100)',
      greenThreshold: 30,
      yellowThreshold: 70,
    },
    {
      icon: TrendingUp,
      title: 'Port Disruption Index',
      description: 'Overall operational disruption level',
      data: data.disruptionIndex,
      yAxisLabel: 'Disruption Index (%)',
      greenThreshold: 30,
      yellowThreshold: 60,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header with title and risk legend */}
      <AnalyticsHeader />

      {/* Trends*/}
      <div className="grid grid-cols-1 gap-6">
        {trendMetrics.map((metric, idx) => (
          <AnalyticsCard
            key={idx}
            icon={metric.icon}
            title={metric.title}
            description={metric.description}
            data={metric.data}
            yAxisLabel={metric.yAxisLabel}
            greenThreshold={metric.greenThreshold}
            yellowThreshold={metric.yellowThreshold}
          />
        ))}
      </div>

      {/* Forecast Summary Card */}
      <ForecastSummaryCard data={data.forecastSummary} />
    </div>
  );
}