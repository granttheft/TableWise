import { useQuery } from '@tanstack/react-query'
import axios from 'axios'
import api from '@/lib/api'
import type { AuditLogEntry } from '@/types/api'

interface AuditLogItem {
  id: string
  action: string
  performedBy: string
  entityType?: string | null
  entityId?: string | null
  createdAt: string
}

interface PagedAuditLogs {
  items: AuditLogItem[]
  totalCount: number
  pageNumber: number
  pageSize: number
}

/**
 * Son audit kayıtları (Owner). Staff için 403 — boş dizi döner.
 */
export function useAuditLogRecent() {
  return useQuery({
    queryKey: ['audit-log', 'recent'],
    queryFn: async () => {
      try {
        const { data } = await api.get<PagedAuditLogs>('/api/v1/tenant/audit-logs', {
          params: { pageNumber: 1, pageSize: 10 },
        })
        return (data.items ?? []).map(
          (a): AuditLogEntry => ({
            id: a.id,
            userId: '',
            userName: a.performedBy,
            action: a.action,
            entityType: a.entityType ?? '',
            entityId: a.entityId ?? '',
            details: [a.entityType, a.entityId].filter(Boolean).join(' ') || '—',
            timestamp: a.createdAt,
          })
        )
      } catch (e) {
        if (axios.isAxiosError(e) && e.response?.status === 403) {
          return [] as AuditLogEntry[]
        }
        throw e
      }
    },
    refetchInterval: 60000,
  })
}
