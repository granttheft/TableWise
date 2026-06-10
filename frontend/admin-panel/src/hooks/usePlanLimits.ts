import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { PlanLimits } from '@/types/api'

export function usePlanLimits() {
  return useQuery({
    queryKey: ['plan-limits'],
    queryFn: async () => {
      const response = await api.get<PlanLimits>('/api/v1/tenant/me/plan-limits')
      return response.data
    },
    staleTime: 60_000,
    refetchOnWindowFocus: false,
  })
}

export function isAtLimit(current: number, max: number | null): boolean {
  if (max === null) return false
  return current >= max
}

export function isNearLimit(current: number, max: number | null): boolean {
  if (max === null) return false
  return current / max >= 0.8 && current < max
}

export function limitPercentage(current: number, max: number | null): number {
  if (max === null) return 0
  return Math.min(Math.round((current / max) * 100), 100)
}
