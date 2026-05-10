import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { Venue, ApiResponse } from '@/types/api'

export function useVenues() {
  return useQuery({
    queryKey: ['venues'],
    queryFn: async () => {
      const response = await api.get<ApiResponse<Venue[]>>('/api/v1/venues')
      return response.data.data
    },
  })
}

export function usePlanLimits() {
  return useQuery({
    queryKey: ['plan-limits'],
    queryFn: async () => {
      const response = await api.get<ApiResponse<{ maxTables: number | null; currentTableCount: number }>>(
        '/api/v1/tenant/me/plan-limits'
      )
      return response.data.data
    },
  })
}
