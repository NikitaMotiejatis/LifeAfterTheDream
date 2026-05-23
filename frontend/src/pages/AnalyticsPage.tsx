import { AlertTriangle, Anchor, Clock, Cloud, TrendingUp } from 'lucide-react';
import AnalyticsCard from '../components/analytics/AnalyticsCard';
import AnalyticsHeader from '../components/analytics/AnalyticsHeader';
import ForecastSummaryCard from '../components/analytics/ForecastSummaryCard';
import { useAnalytics } from '../hooks/useAnalytics';

export default function AnalyticsPage() {
  const { data } = useAnalytics();

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
          />
        ))}
      </div>

      {data?.forecastSummary && (
        <ForecastSummaryCard data={data.forecastSummary} />
      )}
    </div>
  );
}
