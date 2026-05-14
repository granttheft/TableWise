import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { Rule, RuleTestContext, RuleTestResult } from '@/types/api'

const rulesBase = '/api/v1/rules'

/**
 * Mekana göre kural listesi (query: venueId).
 */
export function useRules(venueId?: string) {
  return useQuery({
    queryKey: ['rules', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<Rule[]>(rulesBase, { params: { venueId } })
      return response.data
    },
    enabled: !!venueId,
  })
}

/**
 * Tek kural detayı.
 */
export function useRule(ruleId?: string) {
  return useQuery({
    queryKey: ['rule', ruleId],
    queryFn: async () => {
      if (!ruleId) return null
      const response = await api.get<Rule>(`${rulesBase}/${ruleId}`)
      return response.data
    },
    enabled: !!ruleId,
  })
}

/**
 * Yeni kural oluşturur (CreateRuleDto; venueId gövdede).
 */
export function useCreateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; rule: Record<string, unknown> }) => {
      const response = await api.post<string>(rulesBase, {
        ...data.rule,
        venueId: data.venueId,
      })
      return response.data
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      void queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Kural başarıyla oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Kural oluşturulamadı')
    },
  })
}

/**
 * Kural günceller.
 */
export function useUpdateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string; rule: Partial<Rule> }) => {
      await api.put(`${rulesBase}/${data.ruleId}`, data.rule)
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      void queryClient.invalidateQueries({ queryKey: ['rule', variables.ruleId] })
      toast.success('Kural başarıyla güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Kural güncellenemedi')
    },
  })
}

/**
 * Kural siler.
 */
export function useDeleteRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string }) => {
      await api.delete(`${rulesBase}/${data.ruleId}`)
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      void queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Kural başarıyla silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Kural silinemedi')
    },
  })
}

/**
 * Kural öncelik sırasını günceller (ReorderRulesDto: rules).
 */
export function useReorderRules() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; ruleIds: string[] }) => {
      const rules = data.ruleIds.map((id, index) => ({
        id,
        priority: (data.ruleIds.length - index) * 10,
      }))
      await api.put(`${rulesBase}/reorder`, { rules })
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Sıralama güncellenemedi')
    },
  })
}

/**
 * Kuralı test eder.
 */
export function useTestRule() {
  return useMutation({
    mutationFn: async (data: { ruleId: string; context: RuleTestContext }) => {
      const response = await api.post<RuleTestResult>(`${rulesBase}/${data.ruleId}/test`, data.context)
      return response.data
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Test başarısız')
    },
  })
}

/**
 * Kural aktif/pasif toggle (gövde yok; sunucu mevcut durumu tersine çevirir).
 */
export function useToggleRuleActive() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string }) => {
      await api.put(`${rulesBase}/${data.ruleId}/toggle`)
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      toast.success('Kural durumu güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'İşlem başarısız')
    },
  })
}
