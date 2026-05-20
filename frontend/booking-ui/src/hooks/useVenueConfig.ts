import { useQuery } from '@tanstack/react-query';
import { getVenueConfig } from '@/lib/api';

export function useVenueConfig(slug: string) {
  return useQuery({
    queryKey: ['venue-config', slug],
    queryFn: () => getVenueConfig(slug),
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 2,
  });
}
