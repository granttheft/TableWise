import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { RuleStats, ApiResponse } from '@/types/api'

export function useRuleStats() {
  return useQuery({
    queryKey: ['rules', 'stats'],
    queryFn: async () => {
      const response = await api.get<ApiResponse<RuleStats[]>>('/api/v1/rules/stats')
      return response.data.data
    },
    refetchInterval: 60000, // 60 seconds
  })
}
