import type { DisruptionIndexSettings } from '../../types/FormulaSettings';
import SettingsCard from './SettingsCard';
import SettingsField from './SettingsField';
import FormulaHint from './FormulaHint';

interface DisruptionIndexCardProps {
  data: DisruptionIndexSettings;
  onChange: (field: keyof DisruptionIndexSettings, value: number) => void;
  errors?: Partial<Record<keyof DisruptionIndexSettings, string>>;
}

export default function DisruptionIndexCard({
  data,
  onChange,
  errors,
}: DisruptionIndexCardProps) {
  const weightSum =
    data.berthWeight +
    data.delayWeight +
    data.customsWeight +
    data.weatherWeight;

  return (
    <SettingsCard title="Port Disruption Index">
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <SettingsField
          label="Berth Weight"
          value={data.berthWeight}
          onChange={(v) => onChange('berthWeight', v)}
          min={0}
          max={1}
          step={0.1}
          error={errors?.berthWeight}
        />
        <SettingsField
          label="Delay Weight"
          value={data.delayWeight}
          onChange={(v) => onChange('delayWeight', v)}
          min={0}
          max={1}
          step={0.1}
          error={errors?.delayWeight}
        />
        <SettingsField
          label="Customs Weight"
          value={data.customsWeight}
          onChange={(v) => onChange('customsWeight', v)}
          min={0}
          max={1}
          step={0.1}
          error={errors?.customsWeight}
        />
        <SettingsField
          label="Weather Weight"
          value={data.weatherWeight}
          onChange={(v) => onChange('weatherWeight', v)}
          min={0}
          max={1}
          step={0.1}
          error={errors?.weatherWeight}
        />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <SettingsField
          label="Green Threshold"
          value={data.green}
          onChange={(v) => onChange('green', v)}
          min={0}
          error={errors?.green}
        />
        <SettingsField
          label="Yellow Threshold"
          value={data.yellow}
          onChange={(v) => onChange('yellow', v)}
          min={0}
          error={errors?.yellow}
        />
      </div>
      <FormulaHint
        text={`Formula: (berth × ${data.berthWeight}) + (delay × ${data.delayWeight}) + (customs × ${data.customsWeight}) + (weather × ${data.weatherWeight})`}
      />
      <p className="text-xs text-gray-500">
        Note: Weights should sum to 1.0 for accurate results. Current sum:{' '}
        {weightSum.toFixed(2)}
      </p>
    </SettingsCard>
  );
}
