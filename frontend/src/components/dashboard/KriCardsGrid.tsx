import MiniSparkline from '../charts/MiniSparkline';
import type { KriCardDto } from '../../types/Dashboard';

interface Props {
  data: KriCardDto[];
}

export default function KriCardsGrid({ data }: Props) {
  return (
    <div>
      <h2 className="text-lg font-semibold text-gray-900 mb-4">
        Key Risk Indicators
      </h2>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {data.map((kri) => (
          <div
            key={kri.id}
            className="rounded-xl p-4 border"
            style={{ backgroundColor: '#FEFCE8', borderColor: '#FFF085' }}
          >
            <h3 className="text-sm font-semibold text-gray-800 mb-1">
              {kri.title}
            </h3>
            <p className="text-3xl font-bold text-amber-500 mb-2">
              {kri.value}
            </p>
            <MiniSparkline
              data={kri.sparkline}
              gradientId={`spark-${kri.id}`}
            />
            <p className="text-xs text-gray-500 mt-2 font-mono bg-white/60 px-2 py-1 rounded">
              {kri.formula}
            </p>
            <div className="flex gap-3 mt-2">
              {kri.thresholds.map((t) => (
                <span
                  key={t.label}
                  className="flex items-center gap-1 text-xs text-gray-600"
                >
                  <span
                    className="w-2.5 h-2.5 rounded-full inline-block"
                    style={{ backgroundColor: t.color }}
                  />
                  {t.severity}: {t.label}
                </span>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
