import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import L from 'leaflet';
import { useAisVessels } from '../../hooks/useAisVessels';

const SHIP_COLORS: Record<string, string> = {
  // Cargo family
  Cargo: '#3b82f6', // blue
  'Cargo (unspecified)': '#3b82f6',
  'Container HazCat A': '#1d4ed8', // darker blue

  // Tanker family
  Tanker: '#ef4444', // red
  'Tanker (unspecified)': '#ef4444',

  // Working vessels
  Tug: '#f97316', // orange
  Pilot: '#8b5cf6', // purple
  Diving: '#06b6d4', // cyan
  SAR: '#22c55e', // green
  Law: '#dc2626', // dark red

  // Passenger family
  Passenger: '#10b981', // emerald
  'Passenger (unspecified)': '#10b981',

  // Leisure
  Sailing: '#eab308', // yellow
  Pleasure: '#ec4899', // pink

  // Unknown / numeric junk — all gray
  Other: '#9ca3af',
  Unknown: '#9ca3af',
  '8000': '#9ca3af',
  '8441': '#9ca3af',
  '92': '#9ca3af',
  '62': '#9ca3af',
  '67': '#9ca3af',
  '75': '#9ca3af',
};

const DEFAULT_COLOR = '#000000ff'; // catches undefined

function shipIcon(cog: number, color: string = DEFAULT_COLOR) {
  return L.divIcon({
    className: '',
    html: `<div style="
      width: 0; height: 0;
      border-left: 5px solid transparent;
      border-right: 5px solid transparent;
      border-bottom: 14px solid ${color};
      transform: rotate(${cog}deg);
      transform-origin: center bottom;
	  filter: drop-shadow(0px 0px 0px black);
    "></div>`,
    iconSize: [10, 14],
    iconAnchor: [5, 7],
  });
}

export default function ShipMap() {
  const { data: vessels = [], isLoading } = useAisVessels();

  return (
    <div
      style={{
        height: '100%',
        width: '100%',
        borderRadius: 8,
        overflow: 'hidden',
      }}
    >
      <MapContainer
        center={[55.7033, 21.1291]}
        zoom={13}
        style={{ height: '100%', width: '100%' }}
        zoomControl={true}
      >
        <TileLayer
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          attribution="&copy; OpenStreetMap contributors"
        />
        {vessels.map((v) => (
          <Marker
            key={v.mmsi}
            position={[v.lat, v.lon]}
            icon={shipIcon(v.cog, SHIP_COLORS[v.shiptype] ?? DEFAULT_COLOR)}
          >
            <Popup>
              <strong>{v.name}</strong>
              <br />
              Type: {v.shiptype || '—'}
              <br />
              Speed: {v.sog} kn
              <br />
              Heading: {v.hdg === 0 ? 'N/A' : `${v.hdg}°`}
              <br />
              {v.destination && <>Dest: {v.destination}</>}
            </Popup>
          </Marker>
        ))}
      </MapContainer>
      {isLoading && (
        <div
          style={{
            position: 'absolute',
            top: 8,
            right: 8,
            background: '#fff',
            padding: '4px 8px',
            borderRadius: 4,
            fontSize: 11,
          }}
        >
          Loading...
        </div>
      )}
    </div>
  );
}
