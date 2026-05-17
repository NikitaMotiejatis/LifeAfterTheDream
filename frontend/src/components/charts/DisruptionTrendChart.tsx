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

function getBarFill(value: number) {
  if (value <= 33) return '#22c55e';
  if (value <= 66) return '#eab308';
  return '#ef4444';
}

interface BarShapeProps {
  x?: number;
  y?: number;
  width?: number;
  height?: number;
  payload?: TrendPointDto;
}

function TrendBarShape({ x, y, width, height, payload }: BarShapeProps) {
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
      fill={getBarFill(payload.value)}
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
}

export default function DisruptionTrendChart({
  data,
  trendTimeFrame,
  onTrendTimeFrameChange,
}: Props) {
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
          data={data}
          margin={{ top: 20, right: 16, bottom: 16, left: -10 }}
        >
          <CartesianGrid strokeDasharray="3 3" vertical={false} />
          <XAxis
            dataKey="label"
            tick={{ fontSize: 10 }}
            interval={
              trendTimeFrame === '30d'
                ? 4
                : trendTimeFrame === '6m'
                  ? 3
                  : trendTimeFrame === '90d' || trendTimeFrame === '1y'
                    ? 1
                    : 2
            }
            label={{
              value: xAxisLabelMap[trendTimeFrame],
              position: 'insideBottomRight',
              offset: -4,
              fontSize: 10,
              fill: '#9ca3af',
            }}
          />
          <YAxis domain={[0, 100]} tick={{ fontSize: 10 }} />
          <Tooltip wrapperStyle={{ zIndex: 10 }} />
          <Bar dataKey="value" shape={<TrendBarShape />} />
        </BarChart>
      </ResponsiveContainer>
      <div className="flex gap-4 mt-2 text-xs text-gray-500">
        <span className="flex items-center gap-1">
          <span className="w-2.5 h-2.5 rounded-full bg-green-500" />
          Low (0-33)
        </span>
        <span className="flex items-center gap-1">
          <span className="w-2.5 h-2.5 rounded-full bg-amber-500" />
          Moderate (34-66)
        </span>
        <span className="flex items-center gap-1">
          <span className="w-2.5 h-2.5 rounded-full bg-red-500" />
          High (67-100)
        </span>
      </div>
    </div>
  );
}
