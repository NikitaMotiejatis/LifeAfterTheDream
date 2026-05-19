import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { BarChart as BarChartIcon } from 'lucide-react';
import type { TrendPointDto, TimeFrame } from '../../types/Dashboard';
import TimeFrameFilter from '../dashboard/TimeFrameFilter';
import { downsample } from '../../utils/downsample';

const MAX_TREND_BARS = 30;

function getBarFill(value: number, greenMax: number, yellowMax: number) {
  if (value <= greenMax) return '#22c55e';
  if (value <= yellowMax) return '#eab308';
  return '#ef4444';
}

interface BarShapeProps {
  x?: number;
  y?: number;
  width?: number;
  height?: number;
  payload?: TrendPointDto;
  greenMax: number;
  yellowMax: number;
}

function TrendBarShape({
  x,
  y,
  width,
  height,
  payload,
  greenMax,
  yellowMax,
}: BarShapeProps) {
  if (x == null || y == null || width == null || height == null || !payload)
    return null;
  const r = 3;
  return (
    <rect
      x={x}
      y={y}
      width={width}
      height={height}
      rx={r}
      ry={r}
      fill={getBarFill(payload.value, greenMax, yellowMax)}
    />
  );
}

const xAxisLabelMap: Record<TimeFrame, string> = {
  '24h': 'Time',
  '7d': 'Date',
  '30d': 'Date',
  '90d': 'Date',
  '6m': 'Date',
  '1y': 'Date',
};

interface Props {
  data: TrendPointDto[];
  trendTimeFrame: TimeFrame;
  onTrendTimeFrameChange: (tf: TimeFrame) => void;
  greenMax: number;
  yellowMax: number;
}

export default function DisruptionTrendChart({
  data,
  trendTimeFrame,
  onTrendTimeFrameChange,
  greenMax,
  yellowMax,
}: Props) {
  const chartData = downsample(data, MAX_TREND_BARS);
  // Show at most 12 labels on X-axis
  const maxLabels = 12;
  const xAxisInterval = Math.max(
    0,
    Math.ceil(chartData.length / maxLabels) - 1,
  );

  // Shorten long labels by dropping year/extra parts
  const tickFormatter = (label: string) => {
    if (!label) return '';
    const parts = label.split(' ');
    return parts.length > 2 ? parts.slice(0, 2).join(' ') : label;
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <BarChartIcon className="w-5 h-5 text-blue-600" />
          <h2 className="text-lg font-semibold text-gray-900">
            Port Disruption Index Trend
          </h2>
        </div>
        <TimeFrameFilter
          value={trendTimeFrame}
          onChange={onTrendTimeFrameChange}
        />
      </div>
      <ResponsiveContainer width="100%" height={300}>
        <BarChart
          data={chartData}
          margin={{ top: 20, right: 16, bottom: 16, left: -10 }}
        >
          <CartesianGrid strokeDasharray="3 3" vertical={false} />
          <XAxis
            dataKey="label"
            tick={{ fontSize: 10 }}
            interval={xAxisInterval}
            tickFormatter={tickFormatter}
            angle={chartData.length > 10 ? -30 : 0}
            textAnchor={chartData.length > 10 ? 'end' : 'middle'}
            height={chartData.length > 10 ? 55 : 30}
            label={{
              value: xAxisLabelMap[trendTimeFrame],
              position: 'insideBottomRight',
              offset: -4,
              fontSize: 10,
              fill: '#9ca3af',
            }}
          />
          <YAxis domain={[0, 100]} tick={{ fontSize: 10 }} />
          <Tooltip
            wrapperStyle={{ zIndex: 10 }}
            formatter={(value) => [Number(value).toFixed(1), 'Average PDI']}
          />
          <Bar
            dataKey="value"
            shape={<TrendBarShape greenMax={greenMax} yellowMax={yellowMax} />}
          />
        </BarChart>
      </ResponsiveContainer>
      <div className="flex gap-4 mt-2 text-xs text-gray-500">
        <span className="flex items-center gap-1">
          <span className="w-2.5 h-2.5 rounded-full bg-green-500" />
          Low (0-{greenMax})
        </span>
        <span className="flex items-center gap-1">
          <span className="w-2.5 h-2.5 rounded-full bg-amber-500" />
          Moderate ({greenMax}-{yellowMax})
        </span>
        <span className="flex items-center gap-1">
          <span className="w-2.5 h-2.5 rounded-full bg-red-500" />
          High ({yellowMax}-100)
        </span>
      </div>
    </div>
  );
}
