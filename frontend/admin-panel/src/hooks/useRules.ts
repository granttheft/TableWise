import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { Rule, RuleTestContext } from '@/types/api'
import {
  mapRuleFormToApiPayload,
  type RuleBuilderSubmitPayload,
} from '@/features/rules/utils/mapRuleFormToApiPayload'
import { mapApiRuleDtoToRule, type RuleApiDto } from '@/features/rules/utils/mapApiRuleDto'
import {
  mapRuleTestResultFromApi,
  mapTestContextToApi,
} from '@/features/rules/utils/mapRuleTestApi'

function ruleApiErrorMessage(error: unknown): string {
  if (error instanceof Error && error.message) return error.message
  const err = error as { response?: { data?: { title?: string; detail?: string } } }
  return err.response?.data?.detail || err.response?.data?.title || 'İşlem başarısız'
}

const rulesBase = '/api/v1/rules'

/**
 * Mekana göre kural listesi (query: venueId).
 */
export function useRules(venueId?: string) {
  return useQuery({
    queryKey: ['rules', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<RuleApiDto[]>(rulesBase, { params: { venueId } })
      return response.data.map(mapApiRuleDtoToRule)
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
    mutationFn: async (data: { venueId: string; rule: RuleBuilderSubmitPayload }) => {
      const body = mapRuleFormToApiPayload(data.rule)
      const response = await api.post<string>(rulesBase, {
        venueId: data.venueId,
        ...body,
      })
      return response.data
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      void queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Kural başarıyla oluşturuldu')
    },
    onError: (error: unknown) => {
      toast.error(ruleApiErrorMessage(error) || 'Kural oluşturulamadı')
    },
  })
}

/**
 * Kural günceller.
 */
export function useUpdateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string; rule: RuleBuilderSubmitPayload }) => {
      const body = mapRuleFormToApiPayload(data.rule)
      await api.put(`${rulesBase}/${data.ruleId}`, body)
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      void queryClient.invalidateQueries({ queryKey: ['rule', variables.ruleId] })
      toast.success('Kural başarıyla güncellendi')
    },
    onError: (error: unknown) => {
      toast.error(ruleApiErrorMessage(error) || 'Kural güncellenemedi')
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
    onError: (error: unknown) => {
      toast.error(ruleApiErrorMessage(error) || 'Kural silinemedi')
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
    onError: (error: unknown) => {
      toast.error(ruleApiErrorMessage(error) || 'Sıralama güncellenemedi')
    },
  })
}

/**
 * Kuralı test eder.
 */
export function useTestRule() {
  return useMutation({
    mutationFn: async (data: { ruleId: string; context: RuleTestContext }) => {
      const response = await api.post(`${rulesBase}/${data.ruleId}/test`, mapTestContextToApi(data.context))
      return mapRuleTestResultFromApi(response.data)
    },
    onError: (error: unknown) => {
      toast.error(ruleApiErrorMessage(error) || 'Test başarısız')
    },
  })
}

/**
 * Kaydedilmemiş (taslak) kuralı test eder.
 */
export function useTestRuleDraft() {
  return useMutation({
    mutationFn: async (data: {
      ruleType: string
      conditionsJson: string
      actionsJson: string
      ruleName?: string
      context: RuleTestContext
    }) => {
      const response = await api.post(`${rulesBase}/test-draft`, {
        ruleType: data.ruleType,
        conditionsJson: data.conditionsJson,
        actionsJson: data.actionsJson,
        ruleName: data.ruleName,
        context: mapTestContextToApi(data.context),
      })
      return mapRuleTestResultFromApi(response.data)
    },
    onError: (error: unknown) => {
      toast.error(ruleApiErrorMessage(error) || 'Taslak test başarısız')
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
    onError: (error: unknown) => {
      toast.error(ruleApiErrorMessage(error) || 'İşlem başarısız')
    },
  })
}
