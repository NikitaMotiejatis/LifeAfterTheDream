import { Anchor } from 'lucide-react';
import MiniSparkline from '../charts/MiniSparkline';
import type { PortStatusDto } from '../../types/Dashboard';

interface Props {
  data: PortStatusDto;
}

export default function PortStatusCard({ data }: Props) {
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      <div className="flex items-center gap-2 mb-1">
        <Anchor className="w-5 h-5 text-blue-600" />
        <h2 className="text-lg font-semibold text-gray-900">
          Overall Port Status
        </h2>
      </div>
      <p className="text-sm text-gray-500 mb-4">
        Aggregated risk assessment across all operations
      </p>
      <div className="flex items-end justify-between">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wider mb-1">
            Disruption Index
          </p>
          <p className="text-4xl font-bold text-amber-500">
            {data.disruptionIndex.toFixed(1)}
          </p>
          <p className="text-sm text-amber-600 mt-1 font-medium">
            {data.riskLevel}
          </p>
        </div>
        <div className="w-56">
          <MiniSparkline
            data={data.sparkline}
            color="#f59e0b"
            gradientId="statusFill"
            height={64}
          />
        </div>
      </div>
    </div>
  );
}
