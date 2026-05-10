import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { Customer, CustomerTier } from '@/types/api'

interface CustomersFilters {
  searchTerm?: string
  tier?: string
  isBlacklisted?: boolean
}

/**
 * Müşteri listesini getirir
 */
export function useCustomers(filters?: CustomersFilters) {
  return useQuery({
    queryKey: ['customers', filters],
    queryFn: async () => {
      const params = new URLSearchParams()
      
      if (filters?.searchTerm) {
        params.append('search', filters.searchTerm)
      }
      if (filters?.tier && filters.tier !== 'all') {
        params.append('tier', filters.tier)
      }
      if (filters?.isBlacklisted !== undefined) {
        params.append('isBlacklisted', filters.isBlacklisted.toString())
      }

      const response = await api.get<Customer[]>(
        `/api/v1/customer?${params.toString()}`
      )
      return response.data
    },
  })
}

/**
 * Belirli bir müşteri detayını getirir
 */
export function useCustomer(customerId?: string) {
  return useQuery({
    queryKey: ['customer', customerId],
    queryFn: async () => {
      if (!customerId) return null
      const response = await api.get<Customer>(`/api/v1/customer/${customerId}`)
      return response.data
    },
    enabled: !!customerId,
  })
}

/**
 * Müşteri tier'ını günceller (Owner-only)
 */
export function useUpdateCustomerTier() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { customerId: string; tier: CustomerTier }) => {
      const response = await api.patch<Customer>(
        `/api/v1/customer/${data.customerId}/tier`,
        { tier: data.tier }
      )
      return response.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.customerId] })
      queryClient.invalidateQueries({ queryKey: ['customers'] })
      toast.success('Müşteri tier güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Tier güncellenemedi')
    },
  })
}

/**
 * Müşteri blacklist durumunu günceller
 */
export function useUpdateCustomerBlacklist() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: {
      customerId: string
      isBlacklisted: boolean
      blacklistReason?: string
    }) => {
      const response = await api.patch<Customer>(
        `/api/v1/customer/${data.customerId}/blacklist`,
        {
          isBlacklisted: data.isBlacklisted,
          blacklistReason: data.blacklistReason,
        }
      )
      return response.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.customerId] })
      queryClient.invalidateQueries({ queryKey: ['customers'] })
      toast.success(
        variables.isBlacklisted ? 'Müşteri blacklist\'e eklendi' : 'Müşteri blacklist\'ten çıkarıldı'
      )
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'İşlem başarısız')
    },
  })
}

/**
 * Müşteri notlarını günceller
 */
export function useUpdateCustomerNotes() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { customerId: string; notes: string }) => {
      const response = await api.patch<Customer>(
        `/api/v1/customer/${data.customerId}/notes`,
        { notes: data.notes }
      )
      return response.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.customerId] })
      toast.success('Notlar kaydedildi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Notlar kaydedilemedi')
    },
  })
}
