import { Inbox } from 'lucide-react';

interface Props {
  message?: string;
}

export default function EmptyState({ message = 'No data available.' }: Props) {
  return (
    <div className="bg-gray-50 border border-gray-200 rounded-xl p-6 text-center">
      <Inbox className="w-8 h-8 text-gray-300 mx-auto mb-2" />
      <p className="text-sm text-gray-500">{message}</p>
    </div>
  );
}
