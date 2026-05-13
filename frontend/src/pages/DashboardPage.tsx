import { useDashboard } from '../hooks/useDashboard';
import Spinner from '../components/common/Spinner';
import ErrorCard from '../components/common/ErrorCard';
import PortStatusCard from '../components/dashboard/PortStatusCard';
import WeatherCard from '../components/dashboard/WeatherCard';
import KriCardsGrid from '../components/dashboard/KriCardsGrid';
import VesselScheduleTable from '../components/dashboard/VesselScheduleTable';
import PortMapCard from '../components/dashboard/PortMapCard';
import DisruptionTrendChart from '../components/charts/DisruptionTrendChart';

export default function DashboardPage() {
  const { data, isLoading, isError, refetch } = useDashboard();

  if (isLoading) return <Spinner />;
  if (isError || !data)
    return (
      <ErrorCard
        message="Failed to load dashboard data."
        onRetry={() => refetch()}
      />
    );

  return (
    <div className="space-y-6">
      {/* Overall Status + Weather */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <PortStatusCard data={data.portStatus} />
        <WeatherCard data={data.weather} />
      </div>

      {/* KRI Cards */}
      <KriCardsGrid data={data.kriCards} />

      {/* Disruption Trend */}
      <DisruptionTrendChart data={data.trendData} />

      {/* Port Map */}
      <PortMapCard vessels={data.activeVessels} />

      {/* Vessel Schedule */}
      <VesselScheduleTable data={data.vesselSchedule} />
    </div>
  );
}
