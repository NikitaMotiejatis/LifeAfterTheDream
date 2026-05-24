import axiosInstance from './axiosInstance';

export interface AisVessel {
  mmsi: string;
  name: string;
  lat: number;
  lon: number;
  sog: number;
  hdg: number;        // true heading — use this for icon rotation
  cog: number;        // course over ground — use this if heading is unavailable
  shiptype: string;   // e.g. "Tug", "Pilot", or a number
  destination: string;
}

export async function fetchAisVessels(): Promise<AisVessel[]> {
  const { data: geojson } = await axiosInstance.get('/ais');

  return geojson.features.map((f: any) => ({
    mmsi:        f.properties.mmsi,
    name:        f.properties.shipname ?? f.properties.name ?? 'Unknown',
    lat:         f.properties.lat,
    lon:         f.properties.lon,
    sog:         f.properties.sog ?? 0,
    hdg:         f.properties.hdg === 511 ? 0 : (f.properties.hdg ?? 0),  // 511 = N/A
	cog:		 f.properties.cog === 3600 ? 0 : (f.properties.cog ?? 0),  // 3600 = N/A
    shiptype:    String(f.properties.shiptype ?? ''),
    destination: f.properties.destination ?? '',
  }));
}