import type { WeatherRiskSettings } from '../../types/FormulaSettings';
import SettingsCard from './SettingsCard';
import SettingsField from './SettingsField';
import FormulaHint from './FormulaHint';

interface WeatherRiskCardProps {
  data: WeatherRiskSettings;
  onChange: (field: keyof WeatherRiskSettings, value: number) => void;
  errors?: Partial<Record<keyof WeatherRiskSettings, string>>;
}

export default function WeatherRiskCard({
  data,
  onChange,
  errors,
}: WeatherRiskCardProps) {
  return (
    <SettingsCard title="Weather Risk Score">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
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
        <SettingsField
          label="Wave Height Multiplier"
          value={data.waveHeightMultiplier}
          onChange={(v) => onChange('waveHeightMultiplier', v)}
          min={0}
          error={errors?.waveHeightMultiplier}
        />
      </div>
      <FormulaHint
        text={`Formula: wind_speed + (wave_height × ${data.waveHeightMultiplier})`}
      />
    </SettingsCard>
  );
}
