import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { DashboardStats, ApiResponse } from '@/types/api'

export function useDashboardStats() {
  return useQuery({
    queryKey: ['dashboard', 'stats'],
    queryFn: async () => {
      const response = await api.get<ApiResponse<DashboardStats>>('/api/v1/tenant/me/stats')
      return response.data.data
    },
    refetchInterval: 30000, // 30 seconds
  })
}
