import type { ComponentType } from 'react';
import TrendChart from '../charts/TrendChart';

interface AnalyticsCardProps {
  icon: ComponentType<{ className?: string }>;
  title: string;
  description: string;
  data: any[];
  yAxisLabel: string;
  greenThreshold: number;
  yellowThreshold: number;
}

export default function AnalyticsCard({
  icon: Icon,
  title,
  description,
  data,
  yAxisLabel,
  greenThreshold,
  yellowThreshold,
}: AnalyticsCardProps) {
  const getUnit = () => {
    if (yAxisLabel.includes('%')) return '%';
    if (yAxisLabel.includes('hours')) return 'hrs';
    if (yAxisLabel.includes('Score')) return 'pts';
    return '';
  };
  const unit = getUnit();

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      {/* Card Header*/}
      <div className="flex items-center gap-2 mb-4">
        <Icon className="w-5 h-5 text-blue-600" />
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
          <p className="text-sm text-gray-500 mt-0.5">{description}</p>
        </div>
      </div>

      {/* Chart Container */}
      <TrendChart
        title=""
        data={data}
        yAxisLabel={yAxisLabel}
        greenThreshold={greenThreshold}
        yellowThreshold={yellowThreshold}
      />

      {/* Risk Legend*/}
      <div className="flex gap-4 mt-4 pt-3 border-t border-gray-100 text-xs">
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-green-500"></div>
          <span className="text-gray-600">Low Risk</span>
          <span className="text-gray-400">
            (&lt; {greenThreshold}{unit})
          </span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-yellow-500"></div>
          <span className="text-gray-600">Medium Risk</span>
          <span className="text-gray-400">
            ({greenThreshold}–{yellowThreshold}{unit})
          </span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-red-500"></div>
          <span className="text-gray-600">High Risk</span>
          <span className="text-gray-400">
            (&gt; {yellowThreshold}{unit})
          </span>
        </div>
      </div>
    </div>
  );
}