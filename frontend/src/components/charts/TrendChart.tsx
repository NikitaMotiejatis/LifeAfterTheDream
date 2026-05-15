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

function getYAxisDomain(): [number, number] {
  return [0, 100];
}

export default function TrendChart({
  title,
  data,
  yAxisLabel,
  greenThreshold,
  yellowThreshold,
}: TrendChartProps) {
  const yAxisDomain = getYAxisDomain();
  const [yMin, yMax] = yAxisDomain;

  return (
    <div className="w-full" style={{ height: '320px' }}>
      {title && <h3 className="text-lg font-semibold mb-2">{title}</h3>}
      <ResponsiveContainer width="100%" height="100%">
        <LineChart
          data={data}
          margin={{ top: 20, right: 30, left: 40, bottom: 20 }}
        >
          {/* Risk background zones*/}
          <ReferenceArea
            y1={yMin}
            y2={Math.min(greenThreshold, yMax)}
            fill="#22c55e"
            fillOpacity={0.1}
          />
          <ReferenceArea
            y1={Math.max(yMin, greenThreshold)}
            y2={Math.min(yellowThreshold, yMax)}
            fill="#eab308"
            fillOpacity={0.1}
          />
          <ReferenceArea
            y1={Math.max(yMin, yellowThreshold)}
            y2={yMax}
            fill="#ef4444"
            fillOpacity={0.1}
          />

          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="label" tick={{ fontSize: 11 }} />
          <YAxis
            domain={yAxisDomain}
            label={{
              value: yAxisLabel,
              angle: -90,
              position: 'left',
              offset: 10,
              style: { textAnchor: 'middle' },
            }}
            tick={{ fontSize: 11 }}
            tickCount={6}
            axisLine={true}
          />
          <Tooltip />
          <Legend />
          {/* Historical line (blue) */}
          <Line
            type="monotone"
            dataKey="historical"
            stroke="#2563eb"
            strokeWidth={3}
            dot={{ r: 5, strokeWidth: 0, fill: '#2563eb' }}
            name="Historical"
            connectNulls={false}
          />

          {/* Forecast line (red) */}
          <Line
            type="monotone"
            dataKey="forecast"
            stroke="#dc2626"
            strokeWidth={3}
            strokeDasharray="5 5"
            dot={{ r: 5, strokeWidth: 0, fill: '#dc2626' }}
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