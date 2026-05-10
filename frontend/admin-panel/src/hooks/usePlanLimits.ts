import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { PlanLimits } from '@/types/api'

/**
 * Mevcut planın limitlerini ve kullanımını getirir
 */
export function usePlanLimits() {
  return useQuery({
    queryKey: ['plan-limits'],
    queryFn: async () => {
      const response = await api.get<PlanLimits>('/api/v1/tenant/me/plan-limits')
      return response.data
    },
    staleTime: 60000, // 1 dakika
  })
}
