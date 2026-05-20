import { useQuery } from '@tanstack/react-query';
import { getAvailability } from '@/lib/api';

export function useAvailability(
  slug: string,
  date: string | null,
  partySize: number | null
) {
  return useQuery({
    queryKey: ['availability', slug, date, partySize],
    queryFn: () => getAvailability(slug, date!, partySize!),
    enabled: !!date && !!partySize && partySize > 0,
    staleTime: 1 * 60 * 1000, // 1 minute
    retry: 1,
  });
}
