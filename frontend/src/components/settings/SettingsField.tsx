export default function SettingsField({
  label,
  hint,
  value,
  onChange,
  min,
  max,
  step,
  error,
}: {
  label: string;
  hint?: string;
  value: number;
  onChange: (v: number) => void;
  min?: number;
  max?: number;
  step?: number | 'any';
  error?: string;
}) {
  return (
    <div className="flex flex-col gap-2 flex-1 min-w-45">
      <label className="text-sm font-medium text-gray-700">{label}</label>
      <input
        type="number"
        step={step ?? 'any'}
        min={min}
        max={max}
        className={`w-full rounded-lg border px-4 py-2 text-base text-gray-900 outline-none focus:ring-2 ${
          error
            ? 'border-red-400 focus:ring-red-400'
            : 'border-gray-300 focus:ring-blue-500'
        }`}
        value={value}
        onChange={(e) => onChange(Number(e.target.value))}
      />
      <span
        className={`text-xs min-h-4 ${error ? 'text-red-500' : 'text-gray-500'}`}
      >
        {error ?? hint ?? '\u00A0'}
      </span>
    </div>
  );
}
