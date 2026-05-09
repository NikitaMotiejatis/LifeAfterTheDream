import { Settings, RotateCcw, Save } from 'lucide-react';

interface SettingsHeaderProps {
  isDirty: boolean;
  isSaving: boolean;
  onSave: () => void;
  onReset: () => void;
}

export default function SettingsHeader({
  isDirty,
  isSaving,
  onSave,
  onReset,
}: SettingsHeaderProps) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-4">
      <div className="flex items-center gap-3">
        <Settings className="w-8 h-8 text-blue-600" />
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Formula Settings</h2>
          <p className="text-base text-gray-600">
            Configure risk calculation thresholds and weights
          </p>
        </div>
      </div>
      <div className="flex gap-3">
        <button
          onClick={onReset}
          disabled={isSaving}
          className="inline-flex items-center gap-2 rounded-lg border border-gray-300 px-4 py-2.5 text-base font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
        >
          <RotateCcw className="w-4 h-4" />
          Reset to Default
        </button>
        <button
          onClick={onSave}
          disabled={!isDirty || isSaving}
          className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2.5 text-base font-medium text-white hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed"
        >
          <Save className="w-4 h-4" />
          Save Changes
        </button>
      </div>
    </div>
  );
}
