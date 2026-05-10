import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { Rule, RuleTestContext, RuleTestResult, ApiResponse } from '@/types/api'

export function useRules(venueId?: string) {
  return useQuery({
    queryKey: ['rules', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<ApiResponse<Rule[]>>(`/api/v1/venues/${venueId}/rules`)
      return response.data.data
    },
    enabled: !!venueId,
  })
}

export function useRule(ruleId?: string) {
  return useQuery({
    queryKey: ['rule', ruleId],
    queryFn: async () => {
      if (!ruleId) return null
      const response = await api.get<ApiResponse<Rule>>(`/api/v1/rules/${ruleId}`)
      return response.data.data
    },
    enabled: !!ruleId,
  })
}

export function useCreateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; rule: Omit<Rule, 'id' | 'createdAt' | 'updatedAt' | 'timesTriggered' | 'lastTriggeredAt'> }) => {
      const response = await api.post<ApiResponse<Rule>>(
        `/api/v1/venues/${data.venueId}/rules`,
        data.rule
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Kural başarıyla oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Kural oluşturulamadı')
    },
  })
}

export function useUpdateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string; rule: Partial<Rule> }) => {
      const response = await api.put<ApiResponse<Rule>>(
        `/api/v1/rules/${data.ruleId}`,
        data.rule
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['rule', variables.ruleId] })
      toast.success('Kural başarıyla güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Kural güncellenemedi')
    },
  })
}

export function useDeleteRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string }) => {
      await api.delete(`/api/v1/rules/${data.ruleId}`)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Kural başarıyla silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Kural silinemedi')
    },
  })
}

export function useReorderRules() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; ruleIds: string[] }) => {
      await api.post(`/api/v1/venues/${data.venueId}/rules/reorder`, {
        ruleIds: data.ruleIds,
      })
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Sıralama güncellenemedi')
    },
  })
}

export function useTestRule() {
  return useMutation({
    mutationFn: async (data: { ruleId: string; context: RuleTestContext }) => {
      const response = await api.post<ApiResponse<RuleTestResult>>(
        `/api/v1/rules/${data.ruleId}/test`,
        data.context
      )
      return response.data.data
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Test başarısız')
    },
  })
}

export function useToggleRuleActive() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { ruleId: string; venueId: string; isActive: boolean }) => {
      const response = await api.patch<ApiResponse<Rule>>(
        `/api/v1/rules/${data.ruleId}/toggle`,
        { isActive: data.isActive }
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['rules', variables.venueId] })
      toast.success(variables.isActive ? 'Kural aktifleştirildi' : 'Kural pasifleştirildi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'İşlem başarısız')
    },
  })
}
