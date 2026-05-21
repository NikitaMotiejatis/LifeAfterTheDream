import { Filter } from 'lucide-react';
import type { ComponentType } from 'react';
import { useState } from 'react';
import type { DateTimeRange, TrendDataPoint } from '../../types/AnalyticsIndex';
import TrendChart from '../charts/TrendChart';

interface AnalyticsCardProps {
  icon: ComponentType<{ className?: string }>;
  title: string;
  description: string;
  data: TrendDataPoint[];
  yAxisLabel: string;
  greenThreshold: number;
  yellowThreshold: number;
  yAxisDomain: [number, number];
  onFilterApply?: (range: DateTimeRange) => void;
  isLoading?: boolean;
}

const todayStr = () => new Date().toISOString().split('T')[0];
const tomorrowStr = () => {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return d.toISOString().split('T')[0];
};

export default function AnalyticsCard({
  icon: Icon,
  title,
  description,
  data,
  yAxisLabel,
  greenThreshold,
  yellowThreshold,
  yAxisDomain,
  onFilterApply,
  isLoading = false,
}: AnalyticsCardProps) {
  const [fromDate, setFromDate] = useState(todayStr());
  const [fromTime, setFromTime] = useState('00:00');
  const [toDate, setToDate] = useState(tomorrowStr());
  const [toTime, setToTime] = useState('23:59');
  const [timeErrors, setTimeErrors] = useState<{
    from?: boolean;
    to?: boolean;
  }>({});

  const validateTime = (value: string): boolean =>
    /^([01]\d|2[0-3]):([0-5]\d)$/.test(value);

  const handleFromTimeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setFromTime(value);
    setTimeErrors((prev) => ({
      ...prev,
      from: value.length > 0 && !validateTime(value),
    }));
  };

  const handleToTimeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setToTime(value);
    setTimeErrors((prev) => ({
      ...prev,
      to: value.length > 0 && !validateTime(value),
    }));
  };

  const handleFilterApply = () => {
    const fromTimeValid = validateTime(fromTime);
    const toTimeValid = validateTime(toTime);
    setTimeErrors({ from: !fromTimeValid, to: !toTimeValid });
    if (!fromDate || !toDate || !fromTimeValid || !toTimeValid) return;
    const fromDateTime = new Date(`${fromDate}T${fromTime}`);
    const toDateTime = new Date(`${toDate}T${toTime}`);
    if (fromDateTime > toDateTime) {
      console.warn('From date/time must be before To date/time');
      return;
    }
    onFilterApply?.({ fromDate, fromTime, toDate, toTime });
  };

  const hasFilter = fromDate && toDate && !timeErrors.from && !timeErrors.to;
  const filterDisabled = !hasFilter || isLoading;

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
              <input
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                className="border border-gray-300 rounded-md px-2 py-1 text-sm bg-white focus:outline-none focus:ring-1 focus:ring-blue-500"
                aria-label="From date"
              />
              <div className="relative">
                <input
                  type="text"
                  value={fromTime}
                  onChange={handleFromTimeChange}
                  placeholder="HH:mm"
                  pattern="[0-2][0-9]:[0-5][0-9]"
                  className={`border rounded-md px-2 py-1 w-16 text-sm bg-white text-center focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                    timeErrors.from
                      ? 'border-red-500 bg-red-50'
                      : 'border-gray-300'
                  }`}
                  aria-label="From time (24h format)"
                />
                {timeErrors.from && (
                  <p className="absolute -bottom-4 left-0 text-[10px] text-red-600 whitespace-nowrap">
                    Invalid
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
              <input
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                className="border border-gray-300 rounded-md px-2 py-1 text-sm bg-white focus:outline-none focus:ring-1 focus:ring-blue-500"
                aria-label="To date"
              />
              <div className="relative">
                <input
                  type="text"
                  value={toTime}
                  onChange={handleToTimeChange}
                  placeholder="HH:mm"
                  pattern="[0-2][0-9]:[0-5][0-9]"
                  className={`border rounded-md px-2 py-1 w-16 text-sm bg-white text-center focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                    timeErrors.to
                      ? 'border-red-500 bg-red-50'
                      : 'border-gray-300'
                  }`}
                  aria-label="To time (24h format)"
                />
                {timeErrors.to && (
                  <p className="absolute -bottom-4 left-0 text-[10px] text-red-600 whitespace-nowrap">
                    Invalid
                  </p>
                )}
              </div>
            </div>

            {/* Filter Action Button */}
            <button
              onClick={handleFilterApply}
              disabled={filterDisabled}
              className={`flex items-center gap-1 px-3 py-1 rounded-md text-sm font-medium transition-colors h-[30px] ${
                filterDisabled
                  ? 'bg-gray-200 text-gray-400 cursor-not-allowed'
                  : 'bg-blue-600 text-white hover:bg-blue-700 active:bg-blue-800'
              }`}
            >
              <Filter className="w-3.5 h-3.5" />
              <span>{isLoading ? '...' : 'Filter'}</span>
            </button>
          </div>

          {(!fromDate || !toDate) && (
            <p className="mt-1 text-[11px] text-gray-500">
              Select values to apply filter
            </p>
          )}
        </div>
      </div>

      {isLoading && (
        <div className="absolute inset-0 bg-white/40 backdrop-blur-[0.5px] flex items-center justify-center z-10 rounded-xl">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
        </div>
      )}

      {/* Main Chart Space */}
      <TrendChart
        title=""
        data={data}
        yAxisLabel={yAxisLabel}
        greenThreshold={greenThreshold}
        yellowThreshold={yellowThreshold}
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
