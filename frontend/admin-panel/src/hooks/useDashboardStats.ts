import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { DashboardStats } from '@/types/api'

/**
 * Dashboard üst istatistik kartları (backend: GET /api/v1/tenant/me/stats).
 */
export function useDashboardStats() {
  return useQuery({
    queryKey: ['dashboard', 'stats'],
    queryFn: async () => {
      const { data } = await api.get<DashboardStats>('/api/v1/tenant/me/stats')
      return data
    },
    refetchInterval: 30000,
  })
}
