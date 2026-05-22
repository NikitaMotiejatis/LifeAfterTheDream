import type {
  AnalyticsData,
  DateTimeRange,
  ForecastItem,
  TrendDataPoint,
} from '../types/AnalyticsIndex';

const generateTrendData = (
  historicalValues: number[],
  forecastValues: number[],
): TrendDataPoint[] => {
  const timeLabelsHistorical = [
    '00:00',
    '02:00',
    '04:00',
    '06:00',
    '08:00',
    '10:00',
    '12:00',
    '14:00',
    '16:00',
    '18:00',
    '20:00',
    '22:00',
  ];
  const timeLabelsForecast = [
    '23:00',
    '01:00',
    '03:00',
    '05:00',
    '07:00',
    '09:00',
  ];

  const data: TrendDataPoint[] = [];
  for (let i = 0; i < timeLabelsHistorical.length; i++) {
    data.push({
      label: timeLabelsHistorical[i],
      index: i,
      historical: historicalValues[i],
      forecast: null,
    });
  }
  for (let i = 0; i < timeLabelsForecast.length; i++) {
    data.push({
      label: timeLabelsForecast[i],
      index: i + timeLabelsHistorical.length,
      historical: null,
      forecast: forecastValues[i],
    });
  }
  return data;
};

function generateDynamicTrendData(
  historicalValues: number[],
  forecastValues: number[],
  range: DateTimeRange,
): TrendDataPoint[] {
  const from = new Date(`${range.fromDate}T${range.fromTime}`);
  const to = new Date(`${range.toDate}T${range.toTime}`);
  const now = new Date();

  const totalHours = (to.getTime() - from.getTime()) / (1000 * 3600);
  const data: TrendDataPoint[] = [];

  // Determine uniform point count
  let totalTargetPoints = 14;
  if (totalHours > 72) totalTargetPoints = 18;

  const formatLabel = (date: Date) => {
    if (totalHours <= 36) {
      return date.toLocaleTimeString('lt-LT', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      });
    }

    const dateStr = date.toLocaleDateString('lt-LT', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
    const timeStr = date.toLocaleTimeString('lt-LT', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    });

    return `${dateStr}, ${timeStr}`;
  };

  for (let i = 0; i < totalTargetPoints; i++) {
    const progressRatio = i / (totalTargetPoints - 1 || 1);
    const pointTime = new Date(
      from.getTime() + (to.getTime() - from.getTime()) * progressRatio,
    );

    if (pointTime <= now) {
      data.push({
        label: formatLabel(pointTime),
        index: i,
        historical: historicalValues[i % historicalValues.length],
        forecast: null,
      });
    } else {
      data.push({
        label: formatLabel(pointTime),
        index: i,
        historical: null,
        forecast: forecastValues[i % forecastValues.length],
      });
    }
  }

  return data;
}

export const analyticsMetrics = [
  {
    id: 'berth-occupancy',
    historical: [35, 40, 45, 48, 52, 60, 68, 72, 70, 65, 55, 50],
    forecast: [48, 52, 58, 62, 68, 72],
  },
  {
    id: 'vessel-delay-rate',
    historical: [5, 8, 12, 15, 20, 25, 30, 28, 22, 18, 14, 10],
    forecast: [12, 15, 18, 22, 25, 28],
  },
  {
    id: 'customs-dwell-time',
    historical: [2, 3, 4, 5, 7, 9, 12, 11, 8, 6, 5, 4],
    forecast: [5, 6, 8, 9, 10, 11],
  },
  {
    id: 'weather-risk',
    historical: [20, 25, 30, 35, 40, 45, 55, 60, 50, 40, 35, 30],
    forecast: [35, 40, 45, 50, 55, 60],
  },
  {
    id: 'disruption-index',
    historical: [10, 15, 20, 25, 30, 35, 45, 50, 40, 30, 25, 20],
    forecast: [25, 30, 35, 40, 45, 50],
  },
];

export const mockForecastSummary: ForecastItem[] = [
  {
    title: 'Berth Occupancy',
    status: 'Medium Risk',
    description: 'Stable around 73%',
    statusColor: 'yellow',
  },
  {
    title: 'Vessel Delays',
    status: 'Improving',
    description: 'Decreasing to 15-18% by evening',
    statusColor: 'green',
  },
  {
    title: 'Weather Risk',
    status: 'Improving',
    description: 'Risk score dropping to 35-40',
    statusColor: 'green',
  },
  {
    title: 'Port Disruption',
    status: 'Improving',
    description: 'Trending downward overall',
    statusColor: 'green',
  },
];

export const fetchMockAnalyticsData = async (): Promise<AnalyticsData> => {
  await new Promise((resolve) => setTimeout(resolve, 500));
  const trendDataMap: Record<string, TrendDataPoint[]> = {};
  analyticsMetrics.forEach((metric) => {
    trendDataMap[metric.id] = generateTrendData(
      metric.historical,
      metric.forecast,
    );
  });
  return {
    berthOccupancy: trendDataMap['berth-occupancy'],
    vesselDelayRate: trendDataMap['vessel-delay-rate'],
    customsDwellTime: trendDataMap['customs-dwell-time'],
    weatherRisk: trendDataMap['weather-risk'],
    disruptionIndex: trendDataMap['disruption-index'],
    forecastSummary: mockForecastSummary,
  };
};

export const fetchMockAnalyticsDataWithRange = async (range: DateTimeRange) => {
  await new Promise((resolve) => setTimeout(resolve, 500));
  const from = new Date(`${range.fromDate}T${range.fromTime}`);
  const to = new Date(`${range.toDate}T${range.toTime}`);
  const daysDiff = (to.getTime() - from.getTime()) / (1000 * 3600 * 24);
  const shift = Math.sin(daysDiff) * 5;

  const trendDataMap: Record<string, TrendDataPoint[]> = {};
  analyticsMetrics.forEach((metric) => {
    const shiftedHistorical = metric.historical.map((v) =>
      Math.max(0, Math.round(v + shift)),
    );
    trendDataMap[metric.id] = generateDynamicTrendData(
      shiftedHistorical,
      metric.forecast,
      range,
    );
  });

  return {
    berthOccupancy: trendDataMap['berth-occupancy'],
    vesselDelayRate: trendDataMap['vessel-delay-rate'],
    customsDwellTime: trendDataMap['customs-dwell-time'],
    weatherRisk: trendDataMap['weather-risk'],
    disruptionIndex: trendDataMap['disruption-index'],
    forecastSummary: mockForecastSummary,
  };
};
