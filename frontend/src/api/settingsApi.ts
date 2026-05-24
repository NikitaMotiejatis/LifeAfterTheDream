import type { FormulaSettings } from '../types/FormulaSettings';
import axiosInstance from './axiosInstance';

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

// Thresholds (green/yellow) live in the backend table — see
// IThresholdSettingsService. Backend keys by Kri.Slug.
//
// Fields backend doesn't have (weatherRisk.waveHeightMultiplier,
// disruptionIndex.{berth,delay,customs,weather}Weight) stay in localStorage.

const UI_TO_SLUG = {
  berthOccupancy: 'berth-occupancy',
  vesselDelayRate: 'vessel-delay-rate',
  customsDwellTime: 'customs-dwell-time',
  weatherRisk: 'weather-risk',
  disruptionIndex: 'port-status',
} as const;

const EXTRAS_KEY = 'formulaSettingsExtras';

interface FormulaExtras {
  waveHeightMultiplier: number;
  berthWeight: number;
  delayWeight: number;
  customsWeight: number;
  weatherWeight: number;
}

function defaultExtras(): FormulaExtras {
  return {
    waveHeightMultiplier: DEFAULT_SETTINGS.weatherRisk.waveHeightMultiplier,
    berthWeight: DEFAULT_SETTINGS.disruptionIndex.berthWeight,
    delayWeight: DEFAULT_SETTINGS.disruptionIndex.delayWeight,
    customsWeight: DEFAULT_SETTINGS.disruptionIndex.customsWeight,
    weatherWeight: DEFAULT_SETTINGS.disruptionIndex.weatherWeight,
  };
}

function loadExtras(): FormulaExtras {
  const stored = localStorage.getItem(EXTRAS_KEY);
  if (!stored) return defaultExtras();
  try {
    return { ...defaultExtras(), ...(JSON.parse(stored) as Partial<FormulaExtras>) };
  } catch {
    return defaultExtras();
  }
}

function saveExtras(extras: FormulaExtras) {
  localStorage.setItem(EXTRAS_KEY, JSON.stringify(extras));
}

type ThresholdsResponse = Record<string, { green: number; yellow: number }>;

function pair(
  resp: ThresholdsResponse,
  uiKey: keyof typeof UI_TO_SLUG,
  fallback: { green: number; yellow: number },
) {
  const slug = UI_TO_SLUG[uiKey];
  const t = resp[slug];
  return t ?? fallback;
}

function mergeWithExtras(resp: ThresholdsResponse): FormulaSettings {
  const extras = loadExtras();
  return {
    berthOccupancy: pair(resp, 'berthOccupancy', DEFAULT_SETTINGS.berthOccupancy),
    vesselDelayRate: pair(resp, 'vesselDelayRate', DEFAULT_SETTINGS.vesselDelayRate),
    customsDwellTime: pair(resp, 'customsDwellTime', DEFAULT_SETTINGS.customsDwellTime),
    weatherRisk: {
      ...pair(resp, 'weatherRisk', DEFAULT_SETTINGS.weatherRisk),
      waveHeightMultiplier: extras.waveHeightMultiplier,
    },
    disruptionIndex: {
      ...pair(resp, 'disruptionIndex', DEFAULT_SETTINGS.disruptionIndex),
      berthWeight: extras.berthWeight,
      delayWeight: extras.delayWeight,
      customsWeight: extras.customsWeight,
      weatherWeight: extras.weatherWeight,
    },
  };
}

function toBackendPayload(settings: FormulaSettings): ThresholdsResponse {
  return {
    [UI_TO_SLUG.berthOccupancy]: settings.berthOccupancy,
    [UI_TO_SLUG.vesselDelayRate]: settings.vesselDelayRate,
    [UI_TO_SLUG.customsDwellTime]: {
      green: settings.customsDwellTime.green,
      yellow: settings.customsDwellTime.yellow,
    },
    [UI_TO_SLUG.weatherRisk]: {
      green: settings.weatherRisk.green,
      yellow: settings.weatherRisk.yellow,
    },
    [UI_TO_SLUG.disruptionIndex]: {
      green: settings.disruptionIndex.green,
      yellow: settings.disruptionIndex.yellow,
    },
  };
}

export async function getFormulaSettings(): Promise<FormulaSettings> {
  const resp = await axiosInstance.get<ThresholdsResponse>('/settings/thresholds');
  return mergeWithExtras(resp.data);
}

export async function saveFormulaSettings(
  settings: FormulaSettings,
): Promise<FormulaSettings> {
  saveExtras({
    waveHeightMultiplier: settings.weatherRisk.waveHeightMultiplier,
    berthWeight: settings.disruptionIndex.berthWeight,
    delayWeight: settings.disruptionIndex.delayWeight,
    customsWeight: settings.disruptionIndex.customsWeight,
    weatherWeight: settings.disruptionIndex.weatherWeight,
  });
  const resp = await axiosInstance.put<ThresholdsResponse>(
    '/settings/thresholds',
    toBackendPayload(settings),
  );
  return mergeWithExtras(resp.data);
}

export async function resetFormulaSettings(): Promise<FormulaSettings> {
  localStorage.removeItem(EXTRAS_KEY);
  const resp = await axiosInstance.post<ThresholdsResponse>(
    '/settings/thresholds/reset',
  );
  return mergeWithExtras(resp.data);
}
