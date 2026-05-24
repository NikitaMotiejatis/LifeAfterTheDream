import { Ship, MapPin } from 'lucide-react';
import EmptyState from '../common/EmptyState';
import ShipMap from '../map/ShipMap';
import type { ActiveVesselDto } from '../../types/Dashboard';

const STATUS_COLORS: Record<string, string> = {
  'on-time': '#22c55e',
  delayed: '#ef4444',
  arrived: '#3b82f6',
};

interface Props {
  vessels: ActiveVesselDto[];
}

export default function PortMapCard({ vessels }: Props) {
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      <div className="flex items-center gap-2 mb-1">
        <MapPin className="w-5 h-5 text-blue-600" />
        <h2 className="text-lg font-semibold text-gray-900">
          Port Map – Klaipėda
        </h2>
      </div>
      <p className="text-xs text-gray-500 mb-4">55.7033° N, 21.1291° E</p>
      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        <div className="lg:col-span-3 h-64 rounded-lg overflow-hidden">
          <ShipMap />
        </div>
        <div>
          <h3 className="text-sm font-semibold text-gray-900 mb-3">
            Active Vessels
          </h3>
          {vessels.length === 0 ? (
            <EmptyState message="No active vessels." />
          ) : (
            <div className="space-y-2">
              {vessels.map((v) => (
                <div
                  key={v.name}
                  className="flex items-center gap-2 bg-gray-50 rounded-lg p-2"
                >
                  <Ship
                    className="w-4 h-4"
                    style={{ color: STATUS_COLORS[v.status] ?? '#6b7280' }}
                  />
                  <div className="min-w-0 flex-1">
                    <p className="text-xs font-semibold text-gray-900 truncate">
                      {v.name}
                    </p>
                    <p className="text-xs text-gray-500">{v.berth}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
          <div className="mt-3 flex flex-wrap gap-2 text-xs text-gray-500">
            <span className="flex items-center gap-1">
              <span className="w-2 h-2 rounded-full bg-green-500" />
              On Time
            </span>
            <span className="flex items-center gap-1">
              <span className="w-2 h-2 rounded-full bg-red-500" />
              Delayed
            </span>
            <span className="flex items-center gap-1">
              <span className="w-2 h-2 rounded-full bg-blue-500" />
              Arrived
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
