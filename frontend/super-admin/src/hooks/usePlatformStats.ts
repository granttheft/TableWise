import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { PlatformStatsDto } from '@/types/api'

export function usePlatformStats() {
  return useQuery<PlatformStatsDto>({
    queryKey: ['platform-stats'],
    queryFn: () => api.get<PlatformStatsDto>('/api/platform/stats').then((r) => r.data),
    staleTime: 60_000,
  })
}
