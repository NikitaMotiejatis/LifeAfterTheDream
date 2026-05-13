export default function SettingsCard({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="bg-white rounded-lg border border-gray-200 shadow-sm p-6 flex flex-col gap-4">
      <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
      {children}
    </div>
  );
}
