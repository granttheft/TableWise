import { useQuery } from '@tanstack/react-query'
import axios from 'axios'
import api from '@/lib/api'
import type { RuleStats } from '@/types/api'

interface RuleStatItem {
  ruleId: string
  ruleName: string
  timesTriggered: number
  isActive: boolean
}

/**
 * Kural tetiklenme istatistikleri (Owner). Staff için 403 — boş dizi.
 */
export function useRuleStats() {
  return useQuery({
    queryKey: ['rules', 'stats'],
    queryFn: async () => {
      try {
        const { data } = await api.get<RuleStatItem[]>('/api/v1/rules/stats')
        return (data ?? []).map(
          (r): RuleStats => ({
            ruleId: String(r.ruleId),
            ruleName: r.ruleName,
            timesTriggered: r.timesTriggered,
            lastTriggeredAt: null,
          })
        )
      } catch (e) {
        if (axios.isAxiosError(e) && e.response?.status === 403) {
          return [] as RuleStats[]
        }
        throw e
      }
    },
    refetchInterval: 60000,
  })
}
