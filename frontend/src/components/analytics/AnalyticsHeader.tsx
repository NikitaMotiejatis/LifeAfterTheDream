import { TrendingUp } from 'lucide-react';

export default function AnalyticsHeader() {
  return (
    <div>
      <div className="flex items-center gap-3 mb-1">
        <div className="p-2 bg-blue-50 rounded-xl">
          <TrendingUp className="w-6 h-6 text-blue-600" />
        </div>
        <h1 className="text-2xl font-bold text-gray-900">
          Analytics & Forecasts
        </h1>
      </div>
      <p className="text-gray-500 mt-1 ml-11">
        Real-time port risk monitoring — 24h history + 12h forecast
      </p>
    </div>
  );
}
