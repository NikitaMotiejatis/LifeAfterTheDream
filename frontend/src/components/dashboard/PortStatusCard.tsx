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
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wider mb-1">
            Disruption Index
          </p>
          <p className="text-4xl font-bold text-amber-500">
            {data.disruptionIndex.toFixed(1)}
          </p>
          <p className="text-sm text-amber-600 mt-1 font-medium">
            {data.riskLevel} risk
          </p>
        </div>
        <div className="w-72">
          <MiniSparkline
            data={data.sparkline}
            color="#f59e0b"
            gradientId="statusFill"
            height={90}
            greenMax={data.greenMax}
            yellowMax={data.yellowMax}
          />
        </div>
      </div>
      <div className="flex gap-4 mt-4 pt-3">
        <span className="flex items-center gap-1 text-xs text-gray-600">
          <span
            className="w-2.5 h-2.5 rounded-full inline-block"
            style={{ backgroundColor: '#22c55e' }}
          />
          Low: &lt;{data.greenMax}
        </span>
        <span className="flex items-center gap-1 text-xs text-gray-600">
          <span
            className="w-2.5 h-2.5 rounded-full inline-block"
            style={{ backgroundColor: '#eab308' }}
          />
          Medium: {data.greenMax}-{data.yellowMax}
        </span>
        <span className="flex items-center gap-1 text-xs text-gray-600">
          <span
            className="w-2.5 h-2.5 rounded-full inline-block"
            style={{ backgroundColor: '#ef4444' }}
          />
          High: &gt;{data.yellowMax}
        </span>
      </div>
    </div>
  );
}
