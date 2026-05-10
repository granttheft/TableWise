import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { WeeklyReservationData } from '@/types/api'

/**
 * Son 7 gün grafik verisi (backend: GET /api/v1/tenant/me/dashboard-weekly).
 */
export function useWeeklyData() {
  return useQuery({
    queryKey: ['dashboard', 'weekly'],
    queryFn: async () => {
      const { data } = await api.get<WeeklyReservationData[]>(
        '/api/v1/tenant/me/dashboard-weekly'
      )
      return data
    },
    refetchInterval: 60000,
  })
}
