import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { PlanLimits, ApiResponse } from '@/types/api'

/**
 * Mevcut planın limitlerini ve kullanımını getirir
 */
export function usePlanLimits() {
  return useQuery({
    queryKey: ['plan-limits'],
    queryFn: async () => {
      const response = await api.get<ApiResponse<PlanLimits>>('/api/v1/tenant/me/plan-limits')
      return response.data.data
    },
    staleTime: 60000, // 1 dakika
  })
}
