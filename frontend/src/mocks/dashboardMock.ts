import type {
  DashboardData,
  DashboardRange,
  DateRange,
  SparkPoint,
  TimeFrame,
  TrendPointDto,
} from '../types/Dashboard';

/* ── Trend chart (independent query) ── */

function trendLabel(timeFrame: TimeFrame, i: number, len: number): string {
  const now = new Date();
  if (timeFrame === '24h') {
    const h = new Date(now.getTime() - (len - 1 - i) * 3600000);
    return h.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }
  if (timeFrame === '7d') {
    const d = new Date(now);
    d.setDate(d.getDate() - (len - 1 - i));
    return d.toLocaleDateString('en-GB', {
      weekday: 'short',
      day: '2-digit',
      month: 'short',
    });
  }
  if (timeFrame === '30d') {
    const d = new Date(now);
    d.setDate(d.getDate() - (len - 1 - i));
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
  }
  if (timeFrame === '90d') {
    const d = new Date(now);
    d.setDate(d.getDate() - (len - 1 - i) * 7);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
  }
  if (timeFrame === '6m') {
    const d = new Date(now);
    d.setDate(d.getDate() - (len - 1 - i) * 7);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
  }
  // 1y — monthly buckets
  const d = new Date(now);
  d.setMonth(d.getMonth() - (len - 1 - i));
  return d.toLocaleDateString('en-GB', { month: 'short', year: '2-digit' });
}

const trendLengths: Record<TimeFrame, number> = {
  '24h': 24,
  '7d': 7,
  '30d': 30,
  '90d': 13,
  '6m': 26,
  '1y': 12,
};

export function getMockTrend(timeFrame: TimeFrame): TrendPointDto[] {
  const len = trendLengths[timeFrame];
  const seed =
    timeFrame === '24h'
      ? 40
      : timeFrame === '7d'
        ? 38
        : timeFrame === '30d'
          ? 35
          : timeFrame === '90d'
            ? 32
            : timeFrame === '6m'
              ? 30
              : 28;
  return Array.from({ length: len }, (_, i) => ({
    hour: trendLabel(timeFrame, i, len),
    value: Math.round(seed + Math.sin(i * 0.8) * 12 + Math.cos(i * 0.3) * 6),
  }));
}

/* ── Dashboard tiles (driven by global date range) ── */

const rangeLabelMap: Record<Exclude<DashboardRange, 'custom'>, string> = {
  '6h': 'last 6 hours',
  '12h': 'last 12 hours',
  '24h': 'last 24 hours',
  '48h': 'last 48 hours',
  '72h': 'last 72 hours',
  week: 'last week',
  month: 'last month',
  year: 'last year',
};

const rangeSeedMap: Record<Exclude<DashboardRange, 'custom'>, number> = {
  '6h': 1.0,
  '12h': 0.95,
  '24h': 0.9,
  '48h': 0.85,
  '72h': 0.8,
  week: 0.75,
  month: 0.65,
  year: 0.55,
};

function rangeMultiplier(range: DateRange): number {
  if (range.preset === 'custom') return 0.7;
  return rangeSeedMap[range.preset];
}

function sparkLabels(range: DateRange, count: number): string[] {
  const now = new Date();
  const hoursMap: Record<string, number> = {
    '6h': 6,
    '12h': 12,
    '24h': 24,
    '48h': 48,
    '72h': 72,
    week: 168,
    month: 720,
    year: 8760,
  };
  const totalHours =
    range.preset === 'custom'
      ? Math.max(
          1,
          (new Date(range.to ?? now.toISOString()).getTime() -
            new Date(range.from ?? now.toISOString()).getTime()) /
            3600000,
        )
      : (hoursMap[range.preset] ?? 24);

  const labels: string[] = [];
  for (let i = 0; i < count; i++) {
    const t = new Date(
      now.getTime() - totalHours * 3600000 * (1 - i / (count - 1)),
    );
    if (totalHours <= 24) {
      labels.push(
        t.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' }) +
          ' ' +
          t.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' }),
      );
    } else if (totalHours <= 168) {
      labels.push(
        t.toLocaleDateString('en-GB', {
          weekday: 'short',
          hour: '2-digit',
          minute: '2-digit',
        }),
      );
    } else if (totalHours <= 720) {
      labels.push(
        t.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' }),
      );
    } else {
      labels.push(
        t.toLocaleDateString('en-GB', { month: 'short', year: '2-digit' }),
      );
    }
  }
  return labels;
}

/* ── Hardcoded sparkline mock data per range (simulates API responses) ── */

type SparklineDataSet = {
  portStatus: number[];
  berthOccupancy: number[];
  vesselDelay: number[];
  customsDwell: number[];
  weatherRisk: number[];
};

const sparklinesByRange: Record<DashboardRange, SparklineDataSet> = {
  '6h': {
    portStatus: [42, 44, 43, 45, 44, 43, 43.7],
    berthOccupancy: [71, 72, 73, 74, 73, 72, 73.3],
    vesselDelay: [22, 24, 23, 25, 24, 26, 25],
    customsDwell: [34, 35, 36, 35, 37, 36, 36],
    weatherRisk: [20, 19, 21, 20, 22, 21, 21],
  },
  '12h': {
    portStatus: [40, 43, 45, 42, 44, 41, 43, 43.7],
    berthOccupancy: [68, 70, 73, 71, 74, 72, 73, 73.3],
    vesselDelay: [18, 20, 23, 21, 24, 22, 25, 25],
    customsDwell: [30, 32, 35, 33, 36, 34, 36, 36],
    weatherRisk: [17, 19, 22, 20, 21, 19, 21, 21],
  },
  '24h': {
    portStatus: [38, 42, 40, 46, 43, 41, 44, 43.7],
    berthOccupancy: [60, 65, 70, 68, 72, 74, 73.3],
    vesselDelay: [12, 18, 20, 22, 19, 24, 25],
    customsDwell: [28, 30, 32, 34, 33, 35, 36],
    weatherRisk: [15, 18, 16, 20, 19, 22, 21],
  },
  '48h': {
    portStatus: [35, 38, 42, 44, 40, 43, 45, 43.7],
    berthOccupancy: [55, 60, 65, 70, 68, 72, 74, 73.3],
    vesselDelay: [10, 14, 18, 22, 19, 21, 24, 25],
    customsDwell: [24, 28, 30, 34, 32, 35, 36, 36],
    weatherRisk: [12, 15, 18, 22, 19, 20, 22, 21],
  },
  '72h': {
    portStatus: [30, 34, 38, 42, 44, 41, 43, 43.7],
    berthOccupancy: [50, 55, 62, 68, 72, 70, 74, 73.3],
    vesselDelay: [8, 12, 16, 20, 22, 19, 24, 25],
    customsDwell: [20, 24, 28, 32, 34, 33, 36, 36],
    weatherRisk: [10, 13, 16, 20, 22, 19, 21, 21],
  },
  week: {
    portStatus: [28, 32, 36, 40, 44, 42, 43.7],
    berthOccupancy: [48, 54, 60, 66, 72, 70, 73.3],
    vesselDelay: [8, 12, 16, 20, 24, 22, 25],
    customsDwell: [18, 22, 26, 30, 34, 33, 36],
    weatherRisk: [8, 12, 15, 18, 22, 20, 21],
  },
  month: {
    portStatus: [22, 28, 34, 38, 42, 40, 43.7],
    berthOccupancy: [40, 48, 56, 64, 70, 68, 73.3],
    vesselDelay: [6, 10, 14, 18, 22, 20, 25],
    customsDwell: [14, 20, 26, 30, 34, 32, 36],
    weatherRisk: [6, 10, 14, 18, 20, 18, 21],
  },
  year: {
    portStatus: [18, 24, 30, 36, 40, 38, 43.7],
    berthOccupancy: [35, 42, 50, 58, 66, 64, 73.3],
    vesselDelay: [4, 8, 14, 18, 22, 20, 25],
    customsDwell: [12, 18, 24, 30, 34, 32, 36],
    weatherRisk: [5, 8, 12, 16, 20, 18, 21],
  },
  custom: {
    portStatus: [20, 26, 32, 38, 42, 40, 43.7],
    berthOccupancy: [38, 46, 54, 62, 68, 66, 73.3],
    vesselDelay: [5, 10, 14, 18, 22, 20, 25],
    customsDwell: [14, 20, 26, 30, 34, 32, 36],
    weatherRisk: [6, 10, 14, 18, 20, 18, 21],
  },
};

function makeSparkline(values: number[], range: DateRange): SparkPoint[] {
  const labels = sparkLabels(range, values.length);
  return values.map((v, i) => ({
    label: labels[i],
    value: +v.toFixed(1),
  }));
}

function periodLabel(range: DateRange): string {
  if (range.preset === 'custom' && range.from && range.to) {
    const fmt = (iso: string) =>
      new Date(iso).toLocaleDateString('en-GB', {
        day: '2-digit',
        month: 'short',
        hour: '2-digit',
        minute: '2-digit',
      });
    return `${fmt(range.from)} – ${fmt(range.to)}`;
  }
  return (
    rangeLabelMap[range.preset as Exclude<DashboardRange, 'custom'>] ??
    range.preset
  );
}

export function getMockTiles(dateRange: DateRange): DashboardData {
  const m = rangeMultiplier(dateRange);
  const sp = sparklinesByRange[dateRange.preset] ?? sparklinesByRange.custom;
  const di = sp.portStatus[sp.portStatus.length - 1];
  return {
    periodLabel: periodLabel(dateRange),
    portStatus: {
      disruptionIndex: di,
      riskLevel:
        di > 40 ? 'Moderate Risk' : di > 25 ? 'Low Risk' : 'Minimal Risk',
      sparkline: makeSparkline(sp.portStatus, dateRange),
    },
    weather: {
      windSpeedKts: Math.round(15 * m),
      waveHeightM: +(1.2 * m).toFixed(1),
      temperatureC: 8,
      humidityPercent: 72,
      description: 'Moderate wind conditions. All berths operational.',
    },
    kriCards: [
      {
        id: 'berth-occupancy',
        title: 'Berth Occupancy',
        value: `${sp.berthOccupancy[sp.berthOccupancy.length - 1].toFixed(1)}%`,
        formula: '11 / 15 × 100',
        thresholds: [
          { label: '<70%', color: '#22c55e', severity: 'Low' },
          { label: '70-90%', color: '#eab308', severity: 'Medium' },
          { label: '>90%', color: '#ef4444', severity: 'High' },
        ],
        sparkline: makeSparkline(sp.berthOccupancy, dateRange),
      },
      {
        id: 'vessel-delay-rate',
        title: 'Vessel Delay Rate',
        value: `${sp.vesselDelay[sp.vesselDelay.length - 1].toFixed(1)}%`,
        formula: '3 / 12 × 100',
        thresholds: [
          { label: '<10%', color: '#22c55e', severity: 'Low' },
          { label: '10-25%', color: '#eab308', severity: 'Medium' },
          { label: '>25%', color: '#ef4444', severity: 'High' },
        ],
        sparkline: makeSparkline(sp.vesselDelay, dateRange),
      },
      {
        id: 'customs-dwell-time',
        title: 'Customs Dwell Time',
        value: `${sp.customsDwell[sp.customsDwell.length - 1].toFixed(1)}h`,
        formula: 'avg_hours_in_customs',
        thresholds: [
          { label: '<24h', color: '#22c55e', severity: 'Low' },
          { label: '24-72h', color: '#eab308', severity: 'Medium' },
          { label: '>72h', color: '#ef4444', severity: 'High' },
        ],
        sparkline: makeSparkline(sp.customsDwell, dateRange),
      },
      {
        id: 'weather-risk-score',
        title: 'Weather Risk Score',
        value: sp.weatherRisk[sp.weatherRisk.length - 1].toFixed(1),
        formula: '15 + (1.2 × 5)',
        thresholds: [
          { label: '<20', color: '#22c55e', severity: 'Low' },
          { label: '20-40', color: '#eab308', severity: 'Medium' },
          { label: '>40', color: '#ef4444', severity: 'High' },
        ],
        sparkline: makeSparkline(sp.weatherRisk, dateRange),
      },
    ],
    activeVessels: [
      { name: 'MSC AURORA', berth: 'B-7', status: 'on-time' },
      { name: 'BALTIC STAR', berth: 'B-3', status: 'delayed' },
      { name: 'NORD EXPRESS', berth: 'B-12', status: 'arrived' },
      { name: 'PETROBALTIC', berth: 'B-1', status: 'delayed' },
      { name: 'KLAIPĖDA EXPRESS', berth: 'B-9', status: 'on-time' },
    ],
    trendData: [],
    vesselSchedule: [
      {
        name: 'MSC AURORA',
        imo: 'IMO 9312345',
        type: 'Container',
        fuelType: 'HFO',
        cargo: 'Containers (2,400 TEU)',
        berth: 'B-7',
        eta: '14:30',
        status: 'On Time',
      },
      {
        name: 'BALTIC STAR',
        imo: 'IMO 9456789',
        type: 'Bulk Carrier',
        fuelType: 'VLSFO',
        cargo: 'Grain (45,000 MT)',
        berth: 'B-3',
        eta: '16:45',
        status: 'Delayed',
      },
      {
        name: 'NORD EXPRESS',
        imo: 'IMO 9234567',
        type: 'RoRo',
        fuelType: 'MGO',
        cargo: 'Vehicles (850 units)',
        berth: 'B-12',
        eta: '11:00',
        status: 'Arrived',
      },
      {
        name: 'PETROBALTIC',
        imo: 'IMO 9567890',
        type: 'Tanker',
        fuelType: 'LNG',
        cargo: 'Crude Oil (80,000 MT)',
        berth: 'B-1',
        eta: '18:00',
        status: 'Delayed',
      },
      {
        name: 'KLAIPĖDA EXPRESS',
        imo: 'IMO 9345678',
        type: 'General Cargo',
        fuelType: 'HFO',
        cargo: 'Steel Products (12,000 MT)',
        berth: 'B-9',
        eta: '09:15',
        status: 'On Time',
      },
    ],
  };
}
