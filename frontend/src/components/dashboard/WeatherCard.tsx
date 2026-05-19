import { Cloud, Wind, Waves, Thermometer, Droplets } from 'lucide-react';
import type { WeatherDto } from '../../types/Dashboard';

interface Props {
  data: WeatherDto;
}

export default function WeatherCard({ data }: Props) {
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      <div className="flex items-center gap-2 mb-4">
        <Cloud className="w-5 h-5 text-blue-600" />
        <h2 className="text-lg font-semibold text-gray-900">
          Current Weather Conditions
        </h2>
      </div>
      <div className="grid grid-cols-2 gap-4 mb-4">
        <Metric
          icon={Wind}
          label="Wind Speed"
          value={`${data.windSpeedKts.toFixed(1)} kts`}
        />
        <Metric
          icon={Waves}
          label="Wave Height"
          value={`${data.waveHeightM.toFixed(1)} m`}
        />
        <Metric
          icon={Thermometer}
          label="Temperature"
          value={`${data.temperatureC.toFixed(1)} °C`}
        />
        <Metric
          icon={Droplets}
          label="Humidity"
          value={`${data.humidityPercent.toFixed(1)}%`}
        />
      </div>
      <p className="text-xs text-gray-500 bg-gray-50 p-2 rounded">
        {data.description}
      </p>
    </div>
  );
}

function Metric({
  icon: Icon,
  label,
  value,
}: {
  icon: React.ComponentType<{ className?: string }>;
  label: string;
  value: string;
}) {
  return (
    <div className="flex items-center gap-3">
      <div className="bg-blue-50 p-2 rounded-lg">
        <Icon className="w-5 h-5 text-blue-600" />
      </div>
      <div>
        <p className="text-xs text-gray-500">{label}</p>
        <p className="text-lg font-bold text-gray-900">{value}</p>
      </div>
    </div>
  );
}
