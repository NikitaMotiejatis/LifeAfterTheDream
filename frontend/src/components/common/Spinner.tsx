import { Loader2 } from 'lucide-react';

export default function Spinner() {
  return (
    <div className="flex items-center justify-center py-12">
      <Loader2 className="w-8 h-8 text-blue-500 animate-spin" />
    </div>
  );
}
