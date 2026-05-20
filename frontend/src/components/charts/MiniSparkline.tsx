import {
  ResponsiveContainer,
  AreaChart,
  Area,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

import type { SparkPoint } from '../../types/Dashboard';
import { downsample } from '../../utils/downsample';

const MAX_SPARK_POINTS = 50;

function SparkActiveDot({
  cx,
  cy,
  payload,
  color,
  getColor,
}: {
  cx?: number;
  cy?: number;
  payload?: { value: number };
  color: string;
  getColor?: (value: number) => string;
}) {
  if (cx == null || cy == null || !payload) return null;
  const dotColor = getColor ? getColor(payload.value) : color;
  const badgeY = 4;
  return (
    <g style={{ outline: 'none' }}>
      <circle
        cx={cx}
        cy={cy}
        r={3}
        fill="#fff"
        stroke={dotColor}
        strokeWidth={2}
      />
      <line
        x1={cx}
        y1={cy - 4}
        x2={cx}
        y2={badgeY + 16}
        stroke={dotColor}
        strokeWidth={1}
        strokeDasharray="2 2"
        opacity={0.5}
      />
      <rect
        x={cx - 16}
        y={badgeY}
        width={32}
        height={16}
        rx={4}
        fill={dotColor}
      />
      <text
        x={cx}
        y={badgeY + 11}
        textAnchor="middle"
        fill="#fff"
        fontSize={9}
        dominantBaseline="middle"
      >
        {payload.value.toFixed(1)}
      </text>
    </g>
  );
}

interface Props {
  data: SparkPoint[];
  color?: string;
  gradientId?: string;
  height?: number;
  showAxes?: boolean;
  greenMax?: number;
  yellowMax?: number;
}

export default function MiniSparkline({
  data,
  color = '#eab308',
  gradientId = 'sparkFill',
  height = 80,
  showAxes = true,
  greenMax,
  yellowMax,
}: Props) {
  const downsampled = downsample(data, MAX_SPARK_POINTS);
  // Add unique index so Recharts doesn't confuse duplicate labels (e.g. same "dd/MM")
  const chartData = downsampled.map((pt, i) => ({ ...pt, _idx: i }));

  const totalHeight = showAxes ? height + 34 : height + 24;
  const margin = showAxes
    ? { top: 24, right: 8, bottom: 20, left: 8 }
    : { top: 24, right: 8, bottom: 4, left: 8 };

  // Show all labels when data is small; skip labels only when many points
  const maxLabels = 7;
  const xAxisInterval =
    chartData.length <= maxLabels
      ? 0
      : Math.ceil(chartData.length / maxLabels) - 1;

  // Shorten labels: keep only the first part (time or short date)
  const tickFormatter = (label: string) => {
    if (!label) return '';
    const parts = label.split(' ');
    return parts.length > 2 ? parts.slice(0, 2).join(' ') : label;
  };

  const getPointColor = (value: number) => {
    if (greenMax == null || yellowMax == null) return color;
    if (value <= greenMax) return '#22c55e';
    if (value <= yellowMax) return '#eab308';
    return '#ef4444';
  };

  // Build a horizontal linearGradient so each segment between points gets colored
  const strokeGradientId = `${gradientId}-stroke`;
  const segmentStops =
    chartData.length > 1 && greenMax != null
      ? chartData.map((pt, i) => {
          const offset = `${(i / (chartData.length - 1)) * 100}%`;
          return (
            <stop key={i} offset={offset} stopColor={getPointColor(pt.value)} />
          );
        })
      : null;

  const renderDot = (props: {
    cx?: number;
    cy?: number;
    payload?: SparkPoint;
  }) => {
    const { cx, cy, payload } = props;
    if (cx == null || cy == null || !payload) return <></>;
    const dotColor = getPointColor(payload.value);
    return (
      <circle
        cx={cx}
        cy={cy}
        r={3}
        fill={dotColor}
        stroke="#fff"
        strokeWidth={1}
      />
    );
  };

  const strokeColor = segmentStops ? `url(#${strokeGradientId})` : color;

  return (
    <div style={{ overflow: 'visible', position: 'relative' }}>
      <ResponsiveContainer width="100%" height={totalHeight}>
        <AreaChart
          data={chartData}
          margin={margin}
          style={{ outline: 'none', overflow: 'visible' }}
        >
          <defs>
            <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor={color} stopOpacity={0.3} />
              <stop offset="100%" stopColor={color} stopOpacity={0} />
            </linearGradient>
            {segmentStops && (
              <linearGradient id={strokeGradientId} x1="0" y1="0" x2="1" y2="0">
                {segmentStops}
              </linearGradient>
            )}
          </defs>
          {showAxes && (
            <XAxis
              dataKey="_idx"
              type="number"
              domain={[0, chartData.length - 1]}
              tick={{ fontSize: 8, fill: '#6b7280' }}
              axisLine={false}
              tickLine={false}
              ticks={chartData
                .filter(
                  (_, i) =>
                    xAxisInterval === 0 || i % (xAxisInterval + 1) === 0,
                )
                .map((pt) => pt._idx)}
              tickFormatter={(idx: number) => {
                const label = chartData[idx]?.label ?? '';
                return tickFormatter(label);
              }}
            />
          )}
          {showAxes && (
            <YAxis
              tick={{ fontSize: 8, fill: '#6b7280' }}
              axisLine={false}
              tickLine={false}
              width={28}
            />
          )}
          <Tooltip
            cursor={false}
            wrapperStyle={{ visibility: 'hidden', padding: 0 }}
            content={() => <span />}
          />
          <Area
            type="monotone"
            dataKey="value"
            stroke={strokeColor}
            strokeWidth={2}
            fill={`url(#${gradientId})`}
            dot={greenMax != null ? renderDot : false}
            activeDot={(props: any) => (
              <SparkActiveDot
                {...props}
                color={color}
                getColor={greenMax != null ? getPointColor : undefined}
              />
            )}
            isAnimationActive={false}
            style={{ outline: 'none' }}
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}
