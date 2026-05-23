import {
  ResponsiveContainer,
  AreaChart,
  Area,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

import type { SparkPoint } from '../../types/Dashboard';

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
  domain: [number, number];
  color?: string;
  gradientId?: string;
  height?: number;
  showAxes?: boolean;
  greenMax?: number;
  yellowMax?: number;
}

export default function MiniSparkline({
  data,
  domain,
  color = '#eab308',
  gradientId = 'sparkFill',
  height = 80,
  showAxes = true,
  greenMax,
  yellowMax,
}: Props) {
  const chartData = data.map((p) => ({
    label: Date.parse(p.label),
    value: p.value,
  }));

  const totalHeight = showAxes ? height + 34 : height + 24;
  const margin = showAxes
    ? { top: 24, right: 8, bottom: 20, left: 8 }
    : { top: 24, right: 8, bottom: 4, left: 8 };

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
              type="number"
              dataKey="label"
              domain={domain}
              tick={{ fontSize: 8, fill: '#6b7280' }}
              axisLine={false}
              tickLine={false}
              angle={-45}
              textAnchor="end"
              tickFormatter={(label: string) => {
                if (!label) return '';

                const interval = domain[1] - domain[0];
                const day = 24 * 60 * 60 * 1000;
                const month = 30 * day;
                return new Date(label).toLocaleString(undefined, {
                  year: interval >= 3 * month ? 'numeric' : undefined,
                  month: interval > 3 * day ? '2-digit' : undefined,
                  day:
                    3 * day < interval && interval < 3 * month
                      ? '2-digit'
                      : undefined,
                  timeStyle: interval <= 3 * day ? 'short' : undefined,
                });
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
