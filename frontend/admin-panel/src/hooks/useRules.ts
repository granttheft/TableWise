import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { Rule, RuleTestContext, RuleTestResult } from '@/types/api'

export function useRules(venueId?: string) {
  return useQuery({
    queryKey: ['rules', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<Rule[]>(`/api/v1/rule/venue/${venueId}`)
      return response.data
    },
    enabled: !!venueId,
  })
}

export function useRule(ruleId?: string) {
  return useQuery({
    queryKey: ['rule', ruleId],
    queryFn: async () => {
      if (!ruleId) return null
      const response = await api.get<Rule>(`/api/v1/rule/${ruleId}`)
      return response.data
    },
    enabled: !!ruleId,
  })
}

export function useCreateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; rule: Omit<Rule, 'id' | 'createdAt' | 'updatedAt' | 'timesTriggered' | 'lastTriggeredAt'> }) => {
      const response = await api.post<string>(
        `/api/v1/rule/venue/${data.venueId}`,
        data.rule
      )
      return response.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Kural başarıyla oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Kural oluşturulamadı')
    },
  })
}

export function useUpdateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string; rule: Partial<Rule> }) => {
      await api.put(`/api/v1/rule/${data.ruleId}`, data.rule)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['rule', variables.ruleId] })
      toast.success('Kural başarıyla güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Kural güncellenemedi')
    },
  })
}

export function useDeleteRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string }) => {
      await api.delete(`/api/v1/rule/${data.ruleId}`)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Kural başarıyla silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Kural silinemedi')
    },
  })
}

export function useReorderRules() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; ruleIds: string[] }) => {
      await api.post(`/api/v1/rule/venue/${data.venueId}/reorder`, {
        ruleIds: data.ruleIds,
      })
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Sıralama güncellenemedi')
    },
  })
}

export function useTestRule() {
  return useMutation({
    mutationFn: async (data: { ruleId: string; context: RuleTestContext }) => {
      const response = await api.post<RuleTestResult>(
        `/api/v1/rule/${data.ruleId}/test`,
        data.context
      )
      return response.data
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Test başarısız')
    },
  })
}

export function useToggleRuleActive() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string; isActive: boolean }) => {
      await api.patch(`/api/v1/rule/${data.ruleId}/toggle`, { isActive: data.isActive })
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      toast.success(variables.isActive ? 'Kural aktifleştirildi' : 'Kural pasifleştirildi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'İşlem başarısız')
    },
  })
}
