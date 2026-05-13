import { useState } from 'react';
import { useDashboard } from '../hooks/useDashboard';
import Spinner from '../components/common/Spinner';
import ErrorCard from '../components/common/ErrorCard';
import PortStatusCard from '../components/dashboard/PortStatusCard';
import WeatherCard from '../components/dashboard/WeatherCard';
import KriCardsGrid from '../components/dashboard/KriCardsGrid';
import VesselScheduleTable from '../components/dashboard/VesselScheduleTable';
import PortMapCard from '../components/dashboard/PortMapCard';
import DisruptionTrendChart from '../components/charts/DisruptionTrendChart';
import DashboardDateFilter from '../components/dashboard/DashboardDateFilter';
import type { DateRange, TimeFrame } from '../types/Dashboard';

const defaultRange: DateRange = { preset: '24h', from: null, to: null };

export default function DashboardPage() {
  const [dateRange, setDateRange] = useState<DateRange>(defaultRange);
  const [trendTimeFrame, setTrendTimeFrame] = useState<TimeFrame>('24h');

  const { tiles, trend, isLoading, isError, refetch } = useDashboard(
    dateRange,
    trendTimeFrame,
  );

  if (isLoading) return <Spinner />;
  if (isError)
    return (
      <ErrorCard message="Failed to load dashboard data." onRetry={refetch} />
    );

  const tileData = tiles.data;
  const trendData = trend.data ?? [];

  return (
    <div className="space-y-6">
      {/* Header + Global Date Filter */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          {tileData?.periodLabel && (
            <p className="text-sm text-gray-500 mt-0.5">
              Showing data for: {tileData.periodLabel}
            </p>
          )}
        </div>
        <DashboardDateFilter value={dateRange} onChange={setDateRange} />
      </div>

      {/* Overall Status + Weather */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {tileData && <PortStatusCard data={tileData.portStatus} />}
        {tileData && <WeatherCard data={tileData.weather} />}
      </div>

      {/* KRI Cards */}
      {tileData && <KriCardsGrid data={tileData.kriCards} />}

      {/* Disruption Trend (has its own time-frame toggle) */}
      <DisruptionTrendChart
        data={trendData}
        trendTimeFrame={trendTimeFrame}
        onTrendTimeFrameChange={setTrendTimeFrame}
      />

      {/* Port Map */}
      {tileData && <PortMapCard vessels={tileData.activeVessels} />}

      {/* Vessel Schedule */}
      {tileData && <VesselScheduleTable data={tileData.vesselSchedule} />}
    </div>
  );
}
