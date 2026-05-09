export interface Alert {
  id: string;
  kriDefinitionId: string;
  readingId?: string;
  message: string;
  severity: 'info' | 'warning' | 'critical';
  createdAt: string;
  acknowledged: boolean;
}
