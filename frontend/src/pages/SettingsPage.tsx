import { useMemo } from 'react';
import { useSettings } from '../hooks/useSettings';
import { useToast } from '../contexts/ToastContext';
import Spinner from '../components/common/Spinner';
import ErrorCard from '../components/common/ErrorCard';
import SettingsHeader from '../components/settings/SettingsHeader';
import ThresholdCard from '../components/settings/ThresholdCard';
import WeatherRiskCard from '../components/settings/WeatherRiskCard';
import DisruptionIndexCard from '../components/settings/DisruptionIndexCard';
import type { FormulaSettings, ThresholdPair } from '../types/FormulaSettings';

function rangeError(value: number, min: number, max: number, label: string) {
  if (Number.isNaN(value)) return `${label} is required`;
  if (value < min) return `Min ${min}`;
  if (value > max) return `Max ${max}`;
  return undefined;
}

function thresholdErrors(
  pair: ThresholdPair,
  min: number,
  max: number,
  unit: string,
) {
  const errors: Partial<Record<'green' | 'yellow', string>> = {};
  errors.green = rangeError(pair.green, min, max, 'Value');
  errors.yellow = rangeError(pair.yellow, min, max, 'Value');
  if (!errors.green && !errors.yellow && pair.green >= pair.yellow) {
    errors.green = `Must be less than yellow (${unit})`;
  }
  return errors;
}

function validateSettings(s: FormulaSettings) {
  const berthOccupancy = thresholdErrors(s.berthOccupancy, 0, 100, '%');
  const vesselDelayRate = thresholdErrors(s.vesselDelayRate, 0, 100, '%');
  const customsDwellTime = thresholdErrors(
    s.customsDwellTime,
    0,
    10000,
    'hours',
  );

  const weatherRisk: Partial<Record<string, string>> = {
    ...thresholdErrors(s.weatherRisk, 0, 10000, 'score'),
    waveHeightMultiplier: rangeError(
      s.weatherRisk.waveHeightMultiplier,
      0,
      100,
      'Multiplier',
    ),
  };

  const di = s.disruptionIndex;
  const disruptionIndex: Partial<Record<string, string>> = {
    ...thresholdErrors(di, 0, 10000, 'score'),
    berthWeight: rangeError(di.berthWeight, 0, 1, 'Weight'),
    delayWeight: rangeError(di.delayWeight, 0, 1, 'Weight'),
    customsWeight: rangeError(di.customsWeight, 0, 1, 'Weight'),
    weatherWeight: rangeError(di.weatherWeight, 0, 1, 'Weight'),
  };

  // strip undefined entries for cleaner checks
  const clean = <T extends Record<string, string | undefined>>(o: T) =>
    Object.fromEntries(
      Object.entries(o).filter(([, v]) => v !== undefined),
    ) as Partial<Record<string, string>>;

  const cleaned = {
    berthOccupancy: clean(berthOccupancy),
    vesselDelayRate: clean(vesselDelayRate),
    customsDwellTime: clean(customsDwellTime),
    weatherRisk: clean(weatherRisk),
    disruptionIndex: clean(disruptionIndex),
  };

  const hasErrors = Object.values(cleaned).some(
    (section) => Object.keys(section).length > 0,
  );
  return { errors: cleaned, hasErrors };
}

export default function SettingsPage() {
  const {
    settings,
    setSettings,
    isDirty,
    isLoading,
    isSaving,
    error,
    save,
    reset,
  } = useSettings();
  const { showToast } = useToast();

  const { errors, hasErrors } = useMemo(
    () => validateSettings(settings),
    [settings],
  );

  if (isLoading) return <Spinner />;
  if (error) return <ErrorCard message={error} />;

  const handleSave = async () => {
    const ok = await save();
    if (ok) showToast('Settings saved successfully');
  };

  const handleReset = async () => {
    const ok = await reset();
    if (ok) showToast('Settings reset to defaults', 'info');
  };

  const patch = <K extends keyof FormulaSettings>(
    section: K,
    field: string,
    value: number,
  ) => {
    setSettings((prev) => ({
      ...prev,
      [section]: { ...prev[section], [field]: value },
    }));
  };

  return (
    <div className="space-y-6">
      <SettingsHeader
        isDirty={isDirty}
        isSaving={isSaving}
        onSave={handleSave}
        onReset={handleReset}
        hasErrors={hasErrors}
      />

      <ThresholdCard
        title="Berth Occupancy"
        thresholds={settings.berthOccupancy}
        greenHint="Occupancy below this is green"
        yellowHint="Above this is red"
        onChange={(f, v) => patch('berthOccupancy', f, v)}
        min={0}
        max={100}
        errors={errors.berthOccupancy}
      />

      <ThresholdCard
        title="Vessel Delay Rate"
        thresholds={settings.vesselDelayRate}
        greenHint="Delay rate below this is green"
        yellowHint="Above this is red"
        onChange={(f, v) => patch('vesselDelayRate', f, v)}
        min={0}
        max={100}
        errors={errors.vesselDelayRate}
      />

      <ThresholdCard
        title="Customs Dwell Time"
        thresholds={settings.customsDwellTime}
        greenLabel="Green Threshold (hours)"
        yellowLabel="Yellow Threshold (hours)"
        greenHint="Dwell time below this is green"
        yellowHint="Above this is red"
        onChange={(f, v) => patch('customsDwellTime', f, v)}
        min={0}
        max={10000}
        errors={errors.customsDwellTime}
      />

      <WeatherRiskCard
        data={settings.weatherRisk}
        onChange={(f, v) => patch('weatherRisk', f, v)}
        errors={errors.weatherRisk}
      />

      <DisruptionIndexCard
        data={settings.disruptionIndex}
        onChange={(f, v) => patch('disruptionIndex', f, v)}
        errors={errors.disruptionIndex}
      />
    </div>
  );
}
