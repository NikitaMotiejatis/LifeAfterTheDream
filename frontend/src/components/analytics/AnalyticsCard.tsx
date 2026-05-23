import type { ComponentType } from 'react';
import { useState } from 'react';
import TrendChart from '../charts/TrendChart';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import axiosInstance from '../../api/axiosInstance';
import Spinner from '../common/Spinner';
import ErrorCard from '../common/ErrorCard';

interface AnalyticsCardProps {
  id: string;
  icon: ComponentType<{ className?: string }>;
  description: string;
  yAxisLabel: string;
  ymin?: number;
  ymax?: number;
}

interface AnalyticsDto {
  title: string;
  greenMax: number;
  yellowMax: number;
  sparkline: {
    label: string;
    value: number;
  }[];
}

async function fetchAnalytics(
  id: string,
  fromDateTime: string,
  toDateTime: string,
): Promise<AnalyticsDto> {
  const params = {
    from: new Date(fromDateTime).toISOString(),
    to: new Date(toDateTime).toISOString(),
  };

  const response = await axiosInstance
    .get(`/analytics/${id}`, { params })
    .then((r) => r.data as AnalyticsDto);

  return response;
}

export default function AnalyticsCard({
  id,
  icon: Icon,
  description,
  yAxisLabel,
  ymin,
  ymax,
}: AnalyticsCardProps) {
  const now = new Date();
  const oneDayAgo = new Date(now.getTime() - 24 * 60 * 60 * 1000);

  const formatToDateTimeLocal = (date: Date) => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const [fromDateTime, setFromDateTime] = useState(
    formatToDateTimeLocal(oneDayAgo),
  );
  const [toDateTime, setToDateTime] = useState(formatToDateTimeLocal(now));

  const [timeErrors, setTimeErrors] = useState<{
    from?: boolean;
    to?: boolean;
  }>({});

  const analytics = useQuery({
    queryKey: [id + '-analytics', { fromDateTime, toDateTime }],
    queryFn: () => fetchAnalytics(id, fromDateTime, toDateTime),
    refetchInterval: 30_000,
    staleTime: 10_000,
    placeholderData: keepPreviousData,
  });

  if (analytics.isLoading) {
    return <Spinner />;
  }
  if (analytics.isError) {
    return (
      <ErrorCard
        message="Failed to load analytics data."
        onRetry={() => analytics.refetch()}
      />
    );
  }

  const title = analytics.data.title;
  const greenThreshold = analytics.data.greenMax;
  const yellowThreshold = analytics.data.yellowMax;

  const yAxisDomain = computeMetricYDomain(
    analytics.data.sparkline.map((s) => s.value),
    [ymin, ymax],
  );

  const getUnit = () => {
    if (yAxisLabel.includes('%')) return '%';
    if (yAxisLabel.includes('hours')) return 'hrs';
    if (yAxisLabel.includes('Score')) return 'pts';
    return '';
  };
  const unit = getUnit();

  return (
    <div className="relative bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4 mb-6">
        {/* Title and Description */}
        <div className="flex items-start gap-2 min-w-0 max-w-md">
          <Icon className="w-5 h-5 text-blue-600 shrink-0 mt-0.5" />
          <div className="min-w-0">
            <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
            <p className="text-sm text-gray-500 mt-0.5">{description}</p>
          </div>
        </div>

        {/* Date/Time Range Filter Toolset */}
        <div className="p-3 bg-gray-50 rounded-lg border border-gray-200 shrink-0 self-start lg:self-auto z-20">
          <div className="flex items-center gap-3 flex-wrap sm:flex-nowrap">
            {/* From segment */}
            <div className="flex items-center gap-1.5">
              <label className="text-xs font-medium text-gray-600 whitespace-nowrap">
                From:
              </label>
              <div className="relative">
                <input
                  type="datetime-local"
                  value={fromDateTime}
                  onChange={(e) => setFromDateTime(e.target.value)}
                  className={`border rounded-md px-2 py-1 text-sm bg-white focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                    timeErrors.to
                      ? 'border-red-500 bg-red-50'
                      : 'border-gray-300'
                  }`}
                  aria-label="To date and time"
                />
                {timeErrors.to && (
                  <p className="absolute -bottom-4 left-0 text-[10px] text-red-600 whitespace-nowrap">
                    Invalid date or time
                  </p>
                )}
              </div>
            </div>

            <span className="text-gray-400 font-medium hidden sm:inline">
              →
            </span>

            {/* To segment */}
            <div className="flex items-center gap-1.5">
              <label className="text-xs font-medium text-gray-600 whitespace-nowrap">
                To:
              </label>
              <div className="relative">
                <input
                  type="datetime-local"
                  value={toDateTime}
                  onChange={(e) => setToDateTime(e.target.value)}
                  className={`border rounded-md px-2 py-1 text-sm bg-white focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                    timeErrors.to
                      ? 'border-red-500 bg-red-50'
                      : 'border-gray-300'
                  }`}
                  aria-label="To date and time"
                />
                {timeErrors.to && (
                  <p className="absolute -bottom-4 left-0 text-[10px] text-red-600 whitespace-nowrap">
                    Invalid date or time
                  </p>
                )}
              </div>
            </div>
          </div>

          <p className="mt-1 text-[11px] text-gray-500">
            Select values to apply filter
          </p>
        </div>
      </div>

      {analytics.isLoading && (
        <div className="absolute inset-0 bg-white/40 backdrop-blur-[0.5px] flex items-center justify-center z-10 rounded-xl">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
        </div>
      )}

      {/* Main Chart Space */}
      <TrendChart
        title=""
        data={analytics.data.sparkline.map((s) => ({
          label: Date.parse(s.label),
          historical: s.value,
        }))}
        yAxisLabel={yAxisLabel}
        greenThreshold={greenThreshold}
        yellowThreshold={yellowThreshold}
        xAxisDomain={[Date.parse(fromDateTime), Date.parse(toDateTime)]}
        yAxisDomain={yAxisDomain}
      />

      {/* Footnote Threshold Legends */}
      <div className="flex gap-4 mt-4 pt-3 border-t border-gray-100 text-xs">
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-green-500"></div>
          <span className="text-gray-600">Low Risk</span>
          <span className="text-gray-400">
            (&lt; {greenThreshold}
            {unit})
          </span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-yellow-500"></div>
          <span className="text-gray-600">Medium Risk</span>
          <span className="text-gray-400">
            ({greenThreshold}–{yellowThreshold}
            {unit})
          </span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-red-500"></div>
          <span className="text-gray-600">High Risk</span>
          <span className="text-gray-400">
            (&gt; {yellowThreshold}
            {unit})
          </span>
        </div>
      </div>
    </div>
  );
}

function computeMetricYDomain(
  data: number[],
  yDomain: [number?, number?],
): [number, number] {
  const [ymin, ymax] = yDomain;

  return [
    ymin ?? Math.floor(0.9 * Math.min(...data)),
    ymax ?? Math.ceil(1.1 * Math.max(...data)),
  ];
}
