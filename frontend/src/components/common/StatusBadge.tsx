interface Props {
  status: string;
}

const STATUS_STYLES: Record<string, string> = {
  'On Time': 'bg-green-100 text-green-700',
  Delayed: 'bg-red-100 text-red-700',
  Arrived: 'bg-blue-100 text-blue-700',
};

export default function StatusBadge({ status }: Props) {
  return (
    <span
      className={`px-2.5 py-0.5 rounded-full text-xs font-medium ${STATUS_STYLES[status] ?? 'bg-gray-100 text-gray-700'}`}
    >
      {status}
    </span>
  );
}
