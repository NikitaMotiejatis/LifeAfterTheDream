import { useState, useEffect, useCallback } from 'react';
import type { FormulaSettings } from '../types/FormulaSettings';
import {
  getFormulaSettings,
  saveFormulaSettings,
  resetFormulaSettings,
  DEFAULT_SETTINGS,
} from '../api/settingsApi';

export function useSettings() {
  const [settings, setSettings] = useState<FormulaSettings>(DEFAULT_SETTINGS);
  const [saved, setSaved] = useState<FormulaSettings>(DEFAULT_SETTINGS);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isDirty = JSON.stringify(settings) !== JSON.stringify(saved);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await getFormulaSettings();
      setSettings(data);
      setSaved(data);
    } catch {
      setError('Failed to load settings.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const save = async (): Promise<boolean> => {
    setIsSaving(true);
    setError(null);
    try {
      const data = await saveFormulaSettings(settings);
      setSaved(data);
      setSettings(data);
      return true;
    } catch {
      setError('Failed to save settings.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const reset = async (): Promise<boolean> => {
    setIsSaving(true);
    setError(null);
    try {
      const data = await resetFormulaSettings();
      setSettings(data);
      setSaved(data);
      return true;
    } catch {
      setError('Failed to reset settings.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  return {
    settings,
    setSettings,
    isDirty,
    isLoading,
    isSaving,
    error,
    save,
    reset,
  };
}
