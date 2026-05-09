import { AlertTriangle } from 'lucide-react';

interface Props {
  message?: string;
  onRetry?: () => void;
}

export default function ErrorCard({
  message = 'Failed to load data.',
  onRetry,
}: Props) {
  return (
    <div className="bg-red-50 border border-red-200 rounded-xl p-6 text-center">
      <AlertTriangle className="w-8 h-8 text-red-400 mx-auto mb-2" />
      <p className="text-sm text-red-700 mb-3">{message}</p>
      {onRetry && (
        <button
          onClick={onRetry}
          className="text-sm font-medium text-red-600 hover:text-red-800 underline"
        >
          Try again
        </button>
      )}
    </div>
  );
}
