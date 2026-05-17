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
}: {
  cx?: number;
  cy?: number;
  payload?: { value: number };
  color: string;
}) {
  if (cx == null || cy == null || !payload) return null;
  const badgeY = 4;
  return (
    <g style={{ outline: 'none' }}>
      <circle
        cx={cx}
        cy={cy}
        r={3}
        fill="#fff"
        stroke={color}
        strokeWidth={2}
      />
      <line
        x1={cx}
        y1={cy - 4}
        x2={cx}
        y2={badgeY + 16}
        stroke={color}
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
        fill="#f59e0b"
      />
      <text
        x={cx}
        y={badgeY + 11}
        textAnchor="middle"
        fill="#000"
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
}

export default function MiniSparkline({
  data,
  color = '#eab308',
  gradientId = 'sparkFill',
  height = 80,
  showAxes = true,
}: Props) {
  const totalHeight = showAxes ? height + 44 : height + 24;
  const margin = showAxes
    ? { top: 24, right: 8, bottom: 30, left: 8 }
    : { top: 24, right: 8, bottom: 4, left: 8 };
  return (
    <div style={{ overflow: 'visible', position: 'relative' }}>
      <ResponsiveContainer width="100%" height={totalHeight}>
        <AreaChart
          data={data}
          margin={margin}
          style={{ outline: 'none', overflow: 'visible' }}
        >
          <defs>
            <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor={color} stopOpacity={0.3} />
              <stop offset="100%" stopColor={color} stopOpacity={0} />
            </linearGradient>
          </defs>
          {showAxes && (
            <XAxis
              dataKey="label"
              tick={{ fontSize: 8, fill: '#6b7280' }}
              axisLine={false}
              tickLine={false}
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
          <Tooltip content={() => null} cursor={false} />
          <Area
            type="monotone"
            dataKey="value"
            stroke={color}
            strokeWidth={2}
            fill={`url(#${gradientId})`}
            dot={false}
            activeDot={<SparkActiveDot color={color} />}
            style={{ outline: 'none' }}
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}
