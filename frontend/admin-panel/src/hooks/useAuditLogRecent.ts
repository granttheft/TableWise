import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { AuditLogEntry, ApiResponse } from '@/types/api'

export function useAuditLogRecent() {
  return useQuery({
    queryKey: ['audit-log', 'recent'],
    queryFn: async () => {
      const response = await api.get<ApiResponse<AuditLogEntry[]>>(
        '/api/v1/tenant/me/audit-logs?limit=10'
      )
      return response.data.data
    },
    refetchInterval: 60000, // 60 seconds
  })
}
