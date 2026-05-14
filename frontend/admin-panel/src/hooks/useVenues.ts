import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { CreateVenuePayload, Venue } from '@/types/api'

/**
 * Tenant'a ait tüm venue'leri getirir
 */
export function useVenues() {
  return useQuery({
    queryKey: ['venues'],
    queryFn: async () => {
      const response = await api.get<Venue[]>('/api/v1/venue')
      return response.data
    },
    /** Masalar/Kurallar gibi sayfalara geçişte güncel liste; harici oluşturma sonrası 5 dk eski cache kalmasın. */
    staleTime: 0,
    refetchOnMount: 'always',
  })
}

/**
 * Belirli bir venue detayını getirir
 */
export function useVenue(venueId?: string) {
  return useQuery({
    queryKey: ['venue', venueId],
    queryFn: async () => {
      if (!venueId) return null
      const response = await api.get<Venue>(`/api/v1/venue/${venueId}`)
      return response.data
    },
    enabled: !!venueId,
  })
}

/**
 * Yeni venue oluşturur
 */
export function useCreateVenue() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateVenuePayload) => {
      const body: CreateVenuePayload = {
        timeZone: 'Europe/Istanbul',
        slotDurationMinutes: 90,
        depositEnabled: false,
        depositPerPerson: false,
        depositRefundPolicy: 2,
        ...data,
      }
      const response = await api.post<string>('/api/v1/venue', body)
      return response.data
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['venues'] })
      void queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Mekan başarıyla oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Mekan oluşturulamadı')
    },
  })
}

/**
 * Venue günceller
 */
export function useUpdateVenue() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; updates: Partial<Venue> }) => {
      await api.put(`/api/v1/venue/${data.venueId}`, data.updates)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['venues'] })
      queryClient.invalidateQueries({ queryKey: ['venue', variables.venueId] })
      toast.success('Mekan başarıyla güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Mekan güncellenemedi')
    },
  })
}
