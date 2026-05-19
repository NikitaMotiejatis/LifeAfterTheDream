import MiniSparkline from '../charts/MiniSparkline';
import type { KriCardDto } from '../../types/Dashboard';

interface Props {
  data: KriCardDto[];
}

const severityStyles = {
  Low: {
    bg: '#F0FDF4',
    border: '#BBF7D0',
    text: 'text-green-600',
    chart: '#22c55e',
  },
  Medium: {
    bg: '#FEFCE8',
    border: '#FFF085',
    text: 'text-amber-500',
    chart: '#eab308',
  },
  High: {
    bg: '#FEF2F2',
    border: '#FECACA',
    text: 'text-red-600',
    chart: '#ef4444',
  },
};

export default function KriCardsGrid({ data }: Props) {
  return (
    <div>
      <h2 className="text-lg font-semibold text-gray-900 mb-4">
        Key Risk Indicators
      </h2>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {data.map((kri) => {
          const style = severityStyles[kri.severity] ?? severityStyles.Medium;
          return (
            <div
              key={kri.id}
              className="rounded-xl p-4 border"
              style={{ backgroundColor: style.bg, borderColor: style.border }}
            >
              <h3 className="text-sm font-semibold text-gray-800 mb-1">
                {kri.title}
              </h3>
              <p className={`text-3xl font-bold mb-2 ${style.text}`}>
                {kri.value}
              </p>
              <MiniSparkline
                data={kri.sparkline}
                color={style.chart}
                gradientId={`spark-${kri.id}`}
                greenMax={kri.greenMax}
                yellowMax={kri.yellowMax}
              />
              <div className="flex gap-3 mt-1">
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
          );
        })}
      </div>
    </div>
  );
}
