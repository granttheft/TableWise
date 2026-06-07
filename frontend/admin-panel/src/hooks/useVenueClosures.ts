import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'

export interface VenueClosureDto {
  id: string
  venueId: string
  date: string
  isFullDay: boolean
  openTime?: string | null
  closeTime?: string | null
  reason?: string | null
  createdAt: string
}

interface CreateClosurePayload {
  date: string
  reason?: string
  isFullDay: boolean
  openTime?: string
  closeTime?: string
}

export function useVenueClosures(venueId?: string) {
  return useQuery({
    queryKey: ['venue-closures', venueId],
    queryFn: async () => {
      const res = await api.get<VenueClosureDto[]>(`/api/v1/venues/${venueId}/closures`)
      return res.data
    },
    enabled: !!venueId,
  })
}

export function useCreateVenueClosure(venueId?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateClosurePayload) => {
      await api.post(`/api/v1/venues/${venueId}/closures`, {
        startDate: data.date,
        endDate: data.date,
        isFullDay: data.isFullDay,
        reason: data.reason ?? null,
        openTime: data.openTime ?? null,
        closeTime: data.closeTime ?? null,
      })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['venue-closures', venueId] })
      toast.success('Kapalı gün eklendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Kapalı gün eklenemedi')
    },
  })
}

export function useDeleteVenueClosure(venueId?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (closureId: string) => {
      await api.delete(`/api/v1/venues/${venueId}/closures/${closureId}`)
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['venue-closures', venueId] })
      toast.success('Kapalı gün silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Kapalı gün silinemedi')
    },
  })
}
