import { Ship } from 'lucide-react';
import StatusBadge from '../common/StatusBadge';
import EmptyState from '../common/EmptyState';
import type { VesselScheduleDto } from '../../types/Dashboard';

interface Props {
  data: VesselScheduleDto[];
}

export default function VesselScheduleTable({ data }: Props) {
  if (data.length === 0) {
    return <EmptyState message="No vessels scheduled." />;
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
      <div className="p-6 pb-0 flex items-center gap-2">
        <Ship className="w-5 h-5 text-blue-600" />
        <h2 className="text-lg font-semibold text-gray-900">Vessel Schedule</h2>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm mt-4">
          <thead>
            <tr className="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              <th className="px-6 py-3">Vessel</th>
              <th className="px-6 py-3">Type</th>
              <th className="px-6 py-3">Fuel Type</th>
              <th className="px-6 py-3">Cargo</th>
              <th className="px-6 py-3">Berth</th>
              <th className="px-6 py-3">ETA</th>
              <th className="px-6 py-3">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {data.map((v) => (
              <tr key={v.imo} className="hover:bg-gray-50">
                <td className="px-6 py-3">
                  <p className="font-medium text-gray-900">{v.name}</p>
                  <p className="text-xs text-gray-500">{v.imo}</p>
                </td>
                <td className="px-6 py-3 text-gray-700">{v.type}</td>
                <td className="px-6 py-3 text-gray-700">{v.fuelType}</td>
                <td className="px-6 py-3 text-gray-700">{v.cargo}</td>
                <td className="px-6 py-3 text-gray-700">{v.berth}</td>
                <td className="px-6 py-3 text-gray-700">{v.eta}</td>
                <td className="px-6 py-3">
                  <StatusBadge status={v.status} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
