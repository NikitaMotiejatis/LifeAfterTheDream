import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ReferenceArea,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

interface TrendChartProps {
  title: string;
  data: {
    label: number; // UNIX time
    historical: number;
  }[];
  yAxisLabel: string;
  greenThreshold: number;
  yellowThreshold: number;
  xAxisDomain: [number, number]; // Bounds
  yAxisDomain: [number, number]; // Bounds
}

export default function TrendChart({
  title,
  data,
  yAxisLabel,
  greenThreshold,
  yellowThreshold,
  xAxisDomain,
  yAxisDomain,
}: TrendChartProps) {
  const [yMin, yMax] = yAxisDomain;
  const hasForecast = false; //data.some((d) => d.forecast !== null);

  const formatXAxisTick = (tickItem: string) => {
    if (!tickItem) return '';

    try {
      const date = new Date(tickItem);

      const year = String(date.getFullYear()).padStart(4, '0');
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');

      return `${year}/${month}/${day} ${hours}:${minutes}`;
    } catch (e) {
      return tickItem;
    }
  };

  return (
    <div className="w-full" style={{ height: '320px' }}>
      {title && <h3 className="text-lg font-semibold mb-2">{title}</h3>}
      <ResponsiveContainer width="100%" height="100%">
        <LineChart
          data={data}
          margin={{ top: 20, right: 30, left: 40, bottom: 20 }}
        >
          {/* Risk background zones */}
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

          <CartesianGrid
            strokeDasharray="0"
            stroke="#f3f4f6"
            vertical={true}
            horizontal={true}
          />

          <XAxis
            type="number"
            dataKey="label"
            domain={xAxisDomain}
            tickFormatter={formatXAxisTick}
            tick={{ fontSize: 10 }}
            angle={-45}
            textAnchor="end"
            height={70}
            interval={0}
          />
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
          <Tooltip
            labelFormatter={(label: string) => {
              if (!label) return '';
              return new Date(label).toLocaleString(undefined, {
                dateStyle: 'medium',
                timeStyle: 'short',
              });
            }}
          />
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
          {hasForecast && (
            <Line
              type="monotone"
              dataKey="forecast"
              stroke="#dc2626"
              strokeWidth={3}
              strokeDasharray="5 5"
              dot={(props: any) => {
                const { payload } = props;
                if (payload.historical === null) {
                  const { cx, cy } = props;
                  return (
                    <circle
                      cx={cx}
                      cy={cy}
                      r={5}
                      fill="#dc2626"
                      stroke="none"
                    />
                  );
                }
                return null;
              }}
              name="Forecast"
              connectNulls={false}
              isAnimationActive={false}
            />
          )}

          {/* Threshold limits lines */}
          <ReferenceLine
            y={greenThreshold}
            stroke="#22c55e"
            strokeDasharray="4 4"
          />
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
