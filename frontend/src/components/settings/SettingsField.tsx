export default function SettingsField({
  label,
  hint,
  value,
  onChange,
}: {
  label: string;
  hint?: string;
  value: number;
  onChange: (v: number) => void;
}) {
  return (
    <div className="flex flex-col gap-2 flex-1 min-w-45">
      <label className="text-sm font-medium text-gray-700">{label}</label>
      <input
        type="number"
        step="any"
        className="w-full rounded-lg border border-gray-300 px-4 py-2 text-base text-gray-900 outline-none focus:ring-2 focus:ring-blue-500"
        value={value}
        onChange={(e) => onChange(Number(e.target.value))}
      />
      {hint && <span className="text-xs text-gray-500">{hint}</span>}
    </div>
  );
}
