import type { DashboardData } from '../types/Dashboard';

export const mockDashboardData: DashboardData = {
  portStatus: {
    disruptionIndex: 43.7,
    riskLevel: 'Moderate Risk',
    sparkline: [38, 42, 40, 46, 43, 41, 44, 43.7],
  },
  weather: {
    windSpeedKts: 15,
    waveHeightM: 1.2,
    temperatureC: 8,
    humidityPercent: 72,
    description: 'Moderate wind conditions. All berths operational.',
  },
  kriCards: [
    {
      id: 'berth-occupancy',
      title: 'Berth Occupancy',
      value: '73.3%',
      formula: '11 / 15 × 100',
      thresholds: [
        { label: '<70%', color: '#22c55e', severity: 'Low' },
        { label: '70-90%', color: '#eab308', severity: 'Medium' },
        { label: '>90%', color: '#ef4444', severity: 'High' },
      ],
      sparkline: [60, 65, 70, 68, 72, 74, 73.3],
    },
    {
      id: 'vessel-delay-rate',
      title: 'Vessel Delay Rate',
      value: '25.0%',
      formula: '3 / 12 × 100',
      thresholds: [
        { label: '<10%', color: '#22c55e', severity: 'Low' },
        { label: '10-25%', color: '#eab308', severity: 'Medium' },
        { label: '>25%', color: '#ef4444', severity: 'High' },
      ],
      sparkline: [12, 18, 20, 22, 19, 24, 25],
    },
    {
      id: 'customs-dwell-time',
      title: 'Customs Dwell Time',
      value: '36.0h',
      formula: 'avg_hours_in_customs',
      thresholds: [
        { label: '<24h', color: '#22c55e', severity: 'Low' },
        { label: '24-72h', color: '#eab308', severity: 'Medium' },
        { label: '>72h', color: '#ef4444', severity: 'High' },
      ],
      sparkline: [28, 30, 32, 34, 33, 35, 36],
    },
    {
      id: 'weather-risk-score',
      title: 'Weather Risk Score',
      value: '21.0',
      formula: '15 + (1.2 × 5)',
      thresholds: [
        { label: '<20', color: '#22c55e', severity: 'Low' },
        { label: '20-40', color: '#eab308', severity: 'Medium' },
        { label: '>40', color: '#ef4444', severity: 'High' },
      ],
      sparkline: [15, 18, 16, 20, 19, 22, 21],
    },
  ],
  activeVessels: [
    { name: 'MSC AURORA', berth: 'B-7', status: 'on-time' },
    { name: 'BALTIC STAR', berth: 'B-3', status: 'delayed' },
    { name: 'NORD EXPRESS', berth: 'B-12', status: 'arrived' },
    { name: 'PETROBALTIC', berth: 'B-1', status: 'delayed' },
    { name: 'KLAIPĖDA EXPRESS', berth: 'B-9', status: 'on-time' },
  ],
  trendData: Array.from({ length: 24 }, (_, i) => {
    const hour = i.toString().padStart(2, '0') + ':00';
    const values = [
      30, 28, 25, 22, 20, 18, 22, 28, 35, 42, 48, 52, 50, 47, 44, 40, 38, 42,
      45, 48, 44, 40, 36, 32,
    ];
    return { hour, value: values[i] };
  }),
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
