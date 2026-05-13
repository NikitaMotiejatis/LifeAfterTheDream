import type { FormulaSettings } from '../types/FormulaSettings';
// import axiosInstance from './axiosInstance';

export const DEFAULT_SETTINGS: FormulaSettings = {
  berthOccupancy: { green: 70, yellow: 90 },
  vesselDelayRate: { green: 10, yellow: 25 },
  customsDwellTime: { green: 24, yellow: 72 },
  weatherRisk: { green: 20, yellow: 40, waveHeightMultiplier: 5 },
  disruptionIndex: {
    green: 33,
    yellow: 66,
    berthWeight: 0.3,
    delayWeight: 0.3,
    customsWeight: 0.2,
    weatherWeight: 0.2,
  },
};

// TODO: Replace mock implementations with real API calls when backend is ready.
// Each function already matches the expected backend contract.

export async function getFormulaSettings(): Promise<FormulaSettings> {
  // return (await axiosInstance.get<FormulaSettings>('/settings/formula')).data;
  const stored = localStorage.getItem('formulaSettings');
  return stored
    ? (JSON.parse(stored) as FormulaSettings)
    : { ...DEFAULT_SETTINGS };
}

export async function saveFormulaSettings(
  settings: FormulaSettings,
): Promise<FormulaSettings> {
  // return (await axiosInstance.put<FormulaSettings>('/settings/formula', settings)).data;
  localStorage.setItem('formulaSettings', JSON.stringify(settings));
  return settings;
}

export async function resetFormulaSettings(): Promise<FormulaSettings> {
  // return (await axiosInstance.post<FormulaSettings>('/settings/formula/reset')).data;
  localStorage.removeItem('formulaSettings');
  return { ...DEFAULT_SETTINGS };
}
