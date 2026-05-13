import type { RiskLevel } from './RiskLevel';

export interface KriReading {
  id: string;
  kriDefinitionId: string;
  value: number;
  timestamp: string;
  riskLevel: RiskLevel;
}
