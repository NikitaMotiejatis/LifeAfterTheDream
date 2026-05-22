interface ForecastItem {
  title: string;
  status:
    | 'Low Risk'
    | 'Medium Risk'
    | 'High Risk'
    | 'Improving'
    | 'Stable'
    | 'Worsening';
  description: string;
  statusColor: 'green' | 'yellow' | 'red' | 'blue';
}

interface ForecastSummaryCardProps {
  data: ForecastItem[];
}

const statusColors = {
  green: 'bg-green-100 text-green-700',
  yellow: 'bg-yellow-100 text-yellow-700',
  red: 'bg-red-100 text-red-700',
  blue: 'bg-blue-100 text-blue-700',
};

export default function ForecastSummaryCard({
  data,
}: ForecastSummaryCardProps) {
  return (
    <div className="bg-white rounded-2xl shadow-md">
      <div className="p-6 border-b border-gray-200">
        <h2 className="text-xl font-semibold text-gray-900">
          Forecast Summary
        </h2>
        <p className="text-sm text-gray-500 mt-1">Next 12 hours predictions</p>
      </div>

      <div className="p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {data.map((item, idx) => (
            <div key={idx} className="border border-gray-200 rounded-lg p-4">
              <div className="flex items-center justify-between mb-2">
                <p className="font-medium text-gray-900">{item.title}</p>
                <span
                  className={`px-2 py-1 text-xs font-medium rounded-full ${statusColors[item.statusColor]}`}
                >
                  {item.status}
                </span>
              </div>
              <p className="text-sm text-gray-600">{item.description}</p>
            </div>
          ))}
        </div>

        <p className="mt-6 text-xs text-gray-400 border-t border-gray-100 pt-4">
          Updated every 2 hours based on live port data
        </p>
      </div>
    </div>
  );
}
