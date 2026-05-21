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
import type { TrendDataPoint } from '../../types/AnalyticsIndex';

interface TrendChartProps {
  title: string;
  data: TrendDataPoint[];
  yAxisLabel: string;
  greenThreshold: number;
  yellowThreshold: number;
}

function computeYDomain(
  data: TrendDataPoint[],
  greenThreshold: number,
  yellowThreshold: number,
  yAxisLabel: string,
): [number, number] {
  if (
    yAxisLabel.toLowerCase().includes('hours') ||
    yAxisLabel.toLowerCase().includes('dwell')
  ) {
    const values = data
      .flatMap((d) => [d.historical, d.forecast])
      .filter((v): v is number => v !== null);

    if (values.length === 0) return [0, 20];

    const dataMin = Math.min(...values);
    const dataMax = Math.max(...values);
    const min = Math.floor(Math.min(dataMin, greenThreshold) * 0.9);
    const max = Math.ceil(Math.max(dataMax, yellowThreshold) * 1.1);

    return [Math.max(0, min), max];
  }

  return [0, 100];
}

export default function TrendChart({
  title,
  data,
  yAxisLabel,
  greenThreshold,
  yellowThreshold,
}: TrendChartProps) {
  const yAxisDomain = computeYDomain(
    data,
    greenThreshold,
    yellowThreshold,
    yAxisLabel,
  );
  const [yMin, yMax] = yAxisDomain;

  const hasForecast = data.some((d) => d.forecast !== null);

  const formatXAxisTick = (tickItem: string) => {
    if (!tickItem.includes(',')) {
      return tickItem;
    }

    // Split the generator's format: "2026-02-06, 04:00" -> ["2026-02-06", "04:00"]
    const [datePart, timePart] = tickItem.split(', ');

    const currentIndex = data.findIndex((d) => d.label === tickItem);
    const firstMatchIndex = data.findIndex((d) => d.label.startsWith(datePart));

    // Extracts the year number (e.g. "2026")
    const yearMatch = datePart.match(/\b\d{4}\b/);
    const currentYear = yearMatch ? yearMatch[0] : '';

    // Removes the year AND strip out any leading hyphens
    const dateWithoutYear = currentYear
      ? datePart
          .replace(currentYear, '')
          .replace(/^[-\s/.,m\.]+/, '')
          .trim()
      : datePart;

    // If one more year appears
    const uniqueYears = new Set(
      data
        .map((d) => {
          if (!d.label.includes(',')) return null;
          const match = d.label.split(', ')[0].match(/\b\d{4}\b/);
          return match ? match[0] : null;
        })
        .filter(Boolean),
    );
    const isMultiYearDataset = uniqueYears.size > 1;

    let yearChanged = false;
    if (currentIndex > 0 && currentYear) {
      const prevTickItem = data[currentIndex - 1].label;
      if (prevTickItem.includes(',')) {
        const prevYearMatch = prevTickItem.split(', ')[0].match(/\b\d{4}\b/);
        const prevYear = prevYearMatch ? prevYearMatch[0] : '';
        if (currentYear !== prevYear) {
          yearChanged = true;
        }
      }
    }

    const shouldShowYear =
      isMultiYearDataset && (currentIndex === 0 || yearChanged);
    const displayDate = shouldShowYear ? datePart : dateWithoutYear;
    if (currentIndex === 0) {
      return `${displayDate} - ${timePart}`;
    }

    if (yearChanged) {
      return `${displayDate} - ${timePart}`;
    }

    if (currentIndex === firstMatchIndex) {
      return `${displayDate} - ${timePart}`;
    }
    return timePart;
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

          {/* Low-opacity gray grid behind every uniform point */}
          <CartesianGrid
            strokeDasharray="0"
            stroke="#f3f4f6"
            vertical={true}
            horizontal={true}
          />

          <XAxis
            dataKey="label"
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
