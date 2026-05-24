import { useEffect, useRef, useState } from 'react';
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
import { useToast } from '../contexts/ToastContext';
import { evaluateToasts } from '../utils/thresholdToasts';
import type { DateRange, TimeFrame } from '../types/Dashboard';

const defaultRange: DateRange = { preset: '24h', from: null, to: null };

export default function DashboardPage() {
  const [dateRange, setDateRange] = useState<DateRange>(defaultRange);
  const [trendTimeFrame, setTrendTimeFrame] = useState<TimeFrame>('24h');

  const getDomain = (dr: DateRange): [number, number] => {
    const now = Date.now();
    const hour = 60 * 60 * 1000;
    const day = 24 * hour;
    switch (dr.preset) {
      case '6h':
        return [now - 6 * hour, now];
      case '12h':
        return [now - 12 * hour, now];
      case '24h':
        return [now - 24 * hour, now];
      case '48h':
        return [now - 48 * hour, now];
      case '72h':
        return [now - 72 * hour, now];
      case 'week':
        return [now - 7 * day, now];
      case 'month':
        return [now - 30 * day, now];
      case 'year':
        return [now - 365 * day, now];
      case 'custom':
        return [Date.parse(dr.from), Date.parse(dr.to)];
    }
  };

  const { tiles, trend, isLoading, isError, refetch } = useDashboard(
    dateRange,
    trendTimeFrame,
  );

  const { showToast } = useToast();
  const toastedKeysRef = useRef<Set<string>>(new Set());

  useEffect(() => {
    if (tiles.data?.kriCards) {
      evaluateToasts(tiles.data.kriCards, toastedKeysRef.current, showToast);
    }
  }, [tiles.data, showToast]);

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
        {tileData && (
          <PortStatusCard
            data={tileData.portStatus}
            domain={getDomain(dateRange)}
          />
        )}
        {tileData && <WeatherCard data={tileData.weather} />}
      </div>

      {/* KRI Cards */}
      {tileData && (
        <KriCardsGrid data={tileData.kriCards} domain={getDomain(dateRange)} />
      )}

      {/* Disruption Trend (has its own time-frame toggle) */}
      <DisruptionTrendChart
        data={trendData}
        trendTimeFrame={trendTimeFrame}
        onTrendTimeFrameChange={setTrendTimeFrame}
        greenMax={tileData?.portStatus.greenMax ?? 30}
        yellowMax={tileData?.portStatus.yellowMax ?? 60}
      />

      {/* Port Map */}
      {tileData && <PortMapCard vessels={tileData.activeVessels} />}

      {/* Vessel Schedule */}
      {tileData && <VesselScheduleTable data={tileData.vesselSchedule} />}
    </div>
  );
}
