import type { WeatherRiskSettings } from '../../types/FormulaSettings';
import SettingsCard from './SettingsCard';
import SettingsField from './SettingsField';
import FormulaHint from './FormulaHint';

interface WeatherRiskCardProps {
  data: WeatherRiskSettings;
  onChange: (field: keyof WeatherRiskSettings, value: number) => void;
}

export default function WeatherRiskCard({
  data,
  onChange,
}: WeatherRiskCardProps) {
  return (
    <SettingsCard title="Weather Risk Score">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <SettingsField
          label="Green Threshold"
          value={data.green}
          onChange={(v) => onChange('green', v)}
        />
        <SettingsField
          label="Yellow Threshold"
          value={data.yellow}
          onChange={(v) => onChange('yellow', v)}
        />
        <SettingsField
          label="Wave Height Multiplier"
          value={data.waveHeightMultiplier}
          onChange={(v) => onChange('waveHeightMultiplier', v)}
        />
      </div>
      <FormulaHint
        text={`Formula: wind_speed + (wave_height × ${data.waveHeightMultiplier})`}
      />
    </SettingsCard>
  );
}
