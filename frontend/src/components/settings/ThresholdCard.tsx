import type { ThresholdPair } from '../../types/FormulaSettings';
import SettingsCard from './SettingsCard';
import SettingsField from './SettingsField';

interface ThresholdCardProps {
  title: string;
  thresholds: ThresholdPair;
  greenLabel?: string;
  yellowLabel?: string;
  greenHint?: string;
  yellowHint?: string;
  onChange: (field: 'green' | 'yellow', value: number) => void;
  min?: number;
  max?: number;
  errors?: Partial<Record<'green' | 'yellow', string>>;
}

export default function ThresholdCard({
  title,
  thresholds,
  greenLabel = 'Green Threshold (%)',
  yellowLabel = 'Yellow Threshold (%)',
  greenHint,
  yellowHint,
  onChange,
  min,
  max,
  errors,
}: ThresholdCardProps) {
  return (
    <SettingsCard title={title}>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <SettingsField
          label={greenLabel}
          hint={greenHint}
          value={thresholds.green}
          onChange={(v) => onChange('green', v)}
          min={min}
          max={max}
          error={errors?.green}
        />
        <SettingsField
          label={yellowLabel}
          hint={yellowHint}
          value={thresholds.yellow}
          onChange={(v) => onChange('yellow', v)}
          min={min}
          max={max}
          error={errors?.yellow}
        />
      </div>
    </SettingsCard>
  );
}
