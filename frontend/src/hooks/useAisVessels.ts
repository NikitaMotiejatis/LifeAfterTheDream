import { useQuery } from '@tanstack/react-query';
import { fetchAisVessels } from '../api/aisApi';

export function useAisVessels() {
  return useQuery({
    queryKey: ['ais-vessels'],
    queryFn: fetchAisVessels,
    refetchInterval: 30_000, // refresh every 30 seconds
    staleTime: 20_000,
  });
}
