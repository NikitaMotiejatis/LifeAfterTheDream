import { useState, useRef, useEffect, useMemo } from 'react';
import { Calendar, ChevronDown } from 'lucide-react';
import type { DateRange, DashboardRange } from '../../types/Dashboard';

const presets: { value: Exclude<DashboardRange, 'custom'>; label: string }[] = [
  { value: '6h', label: 'Last 6 hours' },
  { value: '12h', label: 'Last 12 hours' },
  { value: '24h', label: 'Last 24 hours' },
  { value: '48h', label: 'Last 48 hours' },
  { value: '72h', label: 'Last 72 hours' },
  { value: 'week', label: 'Last week' },
  { value: 'month', label: 'Last month' },
  { value: 'year', label: 'Last year' },
];

function formatLabel(range: DateRange): string {
  if (range.preset === 'custom' && range.from && range.to) {
    const fmt = (iso: string) => {
      const d = new Date(iso);
      return d.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    };
    return `${fmt(range.from)} – ${fmt(range.to)}`;
  }
  return presets.find((p) => p.value === range.preset)?.label ?? range.preset;
}

function toLocalDatetime(d: Date): string {
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

interface Props {
  value: DateRange;
  onChange: (r: DateRange) => void;
}

export default function DashboardDateFilter({ value, onChange }: Props) {
  const [open, setOpen] = useState(false);
  const [customFrom, setCustomFrom] = useState(value.from ?? '');
  const [customTo, setCustomTo] = useState(value.to ?? '');
  const ref = useRef<HTMLDivElement>(null);

  const nowStr = useMemo(() => toLocalDatetime(new Date()), []);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  function selectPreset(preset: Exclude<DashboardRange, 'custom'>) {
    onChange({ preset, from: null, to: null });
    setOpen(false);
  }

  const customError = useMemo(() => {
    if (!customFrom || !customTo) return null;
    const from = new Date(customFrom);
    const to = new Date(customTo);
    const now = new Date();
    if (from > now) return '"From" cannot be in the future';
    if (to > now) return '"To" cannot be in the future';
    if (to <= from) return '"To" must be after "From"';
    return null;
  }, [customFrom, customTo]);

  const canApplyCustom = customFrom && customTo && !customError;

  function applyCustom() {
    if (canApplyCustom) {
      onChange({ preset: 'custom', from: customFrom, to: customTo });
      setOpen(false);
    }
  }

  return (
    <div className="relative" ref={ref}>
      <button
        onClick={() => setOpen(!open)}
        className="flex items-center gap-2 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 transition-colors"
      >
        <Calendar className="w-4 h-4 text-gray-500" />
        <span className="max-w-96 truncate">{formatLabel(value)}</span>
        <ChevronDown className="w-4 h-4 text-gray-400" />
      </button>

      {open && (
        <div className="absolute right-0 z-30 mt-1 w-104 rounded-lg border border-gray-200 bg-white shadow-lg">
          {/* Presets */}
          <div className="grid grid-cols-2 grid-flow-col grid-rows-4 gap-x-1 gap-y-0.5 p-3 border-b border-gray-100">
            {presets.map((p) => (
              <button
                key={p.value}
                onClick={() => selectPreset(p.value)}
                className={`text-left rounded-md px-3 py-1.5 text-sm hover:bg-gray-50 transition-colors ${
                  value.preset === p.value
                    ? 'text-blue-600 font-semibold bg-blue-50'
                    : 'text-gray-700'
                }`}
              >
                {p.label}
              </button>
            ))}
          </div>

          {/* Custom range — always visible */}
          <div className="p-4 space-y-2">
            <p className="text-xs font-semibold text-gray-500 uppercase tracking-wider">
              Custom range
            </p>
            <div className="grid grid-cols-2 gap-2">
              <label className="block text-xs text-gray-500">
                From
                <input
                  type="datetime-local"
                  max={nowStr}
                  value={customFrom}
                  onChange={(e) => setCustomFrom(e.target.value)}
                  className="mt-0.5 block w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                />
              </label>
              <label className="block text-xs text-gray-500">
                To
                <input
                  type="datetime-local"
                  min={customFrom || undefined}
                  max={nowStr}
                  value={customTo}
                  onChange={(e) => setCustomTo(e.target.value)}
                  className="mt-0.5 block w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                />
              </label>
            </div>
            {customError && (
              <p className="text-xs text-red-500">{customError}</p>
            )}
            <button
              onClick={applyCustom}
              disabled={!canApplyCustom}
              className="w-full rounded-md bg-blue-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-40 disabled:cursor-not-allowed"
            >
              Apply custom range
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
