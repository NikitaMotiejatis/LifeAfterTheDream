import type { KriCardDto } from '../types/Dashboard';

export type ThresholdLevel = 'green' | 'yellow' | 'red';

export function classifyLevel(
  value: number,
  greenMax: number,
  yellowMax: number,
): ThresholdLevel {
  if (value <= greenMax) return 'green';
  if (value <= yellowMax) return 'yellow';
  return 'red';
}

type ShowToast = (
  message: string,
  type: 'success' | 'error' | 'info' | 'warning',
) => void;

export function evaluateToasts(
  cards: KriCardDto[],
  toastedKeys: Set<string>,
  showToast: ShowToast,
  onRedTransition?: (card: KriCardDto, value: number) => void,
) {
  for (const card of cards) {
    const last = card.sparkline[card.sparkline.length - 1];
    if (!last || typeof last.value !== 'number') continue;

    const level = classifyLevel(last.value, card.greenMax, card.yellowMax);
    if (level === 'green') continue;

    const key = `${card.id}:${level}`;
    if (toastedKeys.has(key)) continue;
    toastedKeys.add(key);

    const valueText = card.value || last.value.toString();
    if (level === 'yellow') {
      showToast(`${card.title} entered yellow zone (${valueText})`, 'warning');
    } else {
      showToast(`${card.title} entered RED zone (${valueText})`, 'error');
      onRedTransition?.(card, last.value);
    }
  }
}
