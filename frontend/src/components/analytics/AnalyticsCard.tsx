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
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      {/* Card Header with Icon */}
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
    </div>
  );
}