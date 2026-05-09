import { useSettings } from '../hooks/useSettings';
import Spinner from '../components/common/Spinner';
import ErrorCard from '../components/common/ErrorCard';
import SettingsHeader from '../components/settings/SettingsHeader';
import ThresholdCard from '../components/settings/ThresholdCard';
import WeatherRiskCard from '../components/settings/WeatherRiskCard';
import DisruptionIndexCard from '../components/settings/DisruptionIndexCard';
import type { FormulaSettings } from '../types/FormulaSettings';

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

  if (isLoading) return <Spinner />;
  if (error) return <ErrorCard message={error} />;

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
        onSave={save}
        onReset={reset}
      />

      <ThresholdCard
        title="Berth Occupancy"
        thresholds={settings.berthOccupancy}
        greenHint="Occupancy below this is green"
        yellowHint="Above this is red"
        onChange={(f, v) => patch('berthOccupancy', f, v)}
      />

      <ThresholdCard
        title="Vessel Delay Rate"
        thresholds={settings.vesselDelayRate}
        greenHint="Delay rate below this is green"
        yellowHint="Above this is red"
        onChange={(f, v) => patch('vesselDelayRate', f, v)}
      />

      <ThresholdCard
        title="Customs Dwell Time"
        thresholds={settings.customsDwellTime}
        greenLabel="Green Threshold (hours)"
        yellowLabel="Yellow Threshold (hours)"
        greenHint="Dwell time below this is green"
        yellowHint="Above this is red"
        onChange={(f, v) => patch('customsDwellTime', f, v)}
      />

      <WeatherRiskCard
        data={settings.weatherRisk}
        onChange={(f, v) => patch('weatherRisk', f, v)}
      />

      <DisruptionIndexCard
        data={settings.disruptionIndex}
        onChange={(f, v) => patch('disruptionIndex', f, v)}
      />
    </div>
  );
}
