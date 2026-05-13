import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ReferenceLine,
  ReferenceArea,
  ResponsiveContainer,
} from 'recharts';

interface TrendDataPoint {
  label: string;
  index: number;
  historical: number | null;
  forecast: number | null;
}

interface TrendChartProps {
  title: string;
  data: TrendDataPoint[];
  yAxisLabel: string;
  greenThreshold: number;
  yellowThreshold: number;
}

export default function TrendChart({
  title,
  data,
  yAxisLabel,
  greenThreshold,
  yellowThreshold,
}: TrendChartProps) {
  return (
    <div className="w-full" style={{ height: '300px' }}>
      {title && <h3 className="text-lg font-semibold mb-2">{title}</h3>}
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={data}>
          {/* Risk background zones */}
          <ReferenceArea
            y1={0}
            y2={greenThreshold}
            fill="#22c55e"
            fillOpacity={0.1}
          />
          <ReferenceArea
            y1={greenThreshold}
            y2={yellowThreshold}
            fill="#eab308"
            fillOpacity={0.1}
          />
          <ReferenceArea
            y1={yellowThreshold}
            y2={100}
            fill="#ef4444"
            fillOpacity={0.1}
          />

          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="label" />
          <YAxis
            label={{ value: yAxisLabel, angle: -90, position: 'insideLeft' }}
          />
          <Tooltip />
          <Legend />

          {/* Historical line (blue) */}
          <Line
            type="monotone"
            dataKey="historical"
            stroke="#2563eb"
            strokeWidth={2}
            dot={{ r: 3 }}
            name="Historical"
            connectNulls={false}
          />

          {/* Forecast line (red) */}
          <Line
            type="monotone"
            dataKey="forecast"
            stroke="#dc2626"
            strokeWidth={2}
            strokeDasharray="5 5"
            dot={{ r: 3 }}
            name="Forecast"
            connectNulls={false}
          />

          {/* Green threshold line */}
          <ReferenceLine
            y={greenThreshold}
            stroke="#22c55e"
            strokeDasharray="4 4"
          />

          {/* Yellow threshold line */}
          <ReferenceLine
            y={yellowThreshold}
            stroke="#eab308"
            strokeDasharray="4 4"
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}