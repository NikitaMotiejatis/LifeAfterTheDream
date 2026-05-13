export default function AnalyticsHeader() {
  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900">Analytics & Forecasts</h1>
      <p className="text-gray-500 mt-1">
        Real-time port risk monitoring — 24h history + 12h forecast
      </p>
      
      {/* Risk Legend */}
      <div className="flex gap-6 mt-4 text-sm">
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-green-500"></div>
          <span className="text-gray-600">Low Risk</span>
          <span className="text-xs text-gray-400">(Below green threshold)</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-yellow-500"></div>
          <span className="text-gray-600">Medium Risk</span>
          <span className="text-xs text-gray-400">(Green to yellow)</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-red-500"></div>
          <span className="text-gray-600">High Risk</span>
          <span className="text-xs text-gray-400">(Above yellow)</span>
        </div>
      </div>
    </div>
  );
}