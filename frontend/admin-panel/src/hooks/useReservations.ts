import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { 
  Reservation, 
  ReservationStatus, 
  ReservationStatusLog,
  ReservationEvaluationResult,
  Customer,
  ApiResponse 
} from '@/types/api'

export function useReservations(venueId?: string, date?: string) {
  return useQuery({
    queryKey: ['reservations', venueId, date],
    queryFn: async () => {
      if (!venueId) return []
      const params = new URLSearchParams()
      if (date) params.append('date', date)
      const response = await api.get<ApiResponse<Reservation[]>>(
        `/api/v1/venues/${venueId}/reservations?${params.toString()}`
      )
      return response.data.data
    },
    enabled: !!venueId,
    refetchInterval: 30000, // 30 seconds
  })
}

export function useReservation(reservationId?: string) {
  return useQuery({
    queryKey: ['reservation', reservationId],
    queryFn: async () => {
      if (!reservationId) return null
      const response = await api.get<ApiResponse<Reservation>>(
        `/api/v1/reservations/${reservationId}`
      )
      return response.data.data
    },
    enabled: !!reservationId,
  })
}

export function useReservationStatusLog(reservationId?: string) {
  return useQuery({
    queryKey: ['reservation-status-log', reservationId],
    queryFn: async () => {
      if (!reservationId) return []
      const response = await api.get<ApiResponse<ReservationStatusLog[]>>(
        `/api/v1/reservations/${reservationId}/status-log`
      )
      return response.data.data
    },
    enabled: !!reservationId,
  })
}

export function useCreateReservation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: {
      venueId: string
      reservation: Partial<Reservation>
      overrideRules?: boolean
    }) => {
      const response = await api.post<ApiResponse<Reservation>>(
        `/api/v1/venues/${data.venueId}/reservations`,
        {
          ...data.reservation,
          overrideRules: data.overrideRules,
        }
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['reservations', variables.venueId] })
      toast.success('Rezervasyon oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Rezervasyon oluşturulamadı')
    },
  })
}

export function useUpdateReservation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: {
      reservationId: string
      venueId: string
      updates: Partial<Reservation>
    }) => {
      const response = await api.put<ApiResponse<Reservation>>(
        `/api/v1/reservations/${data.reservationId}`,
        data.updates
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['reservations', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['reservation', variables.reservationId] })
      toast.success('Rezervasyon güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Rezervasyon güncellenemedi')
    },
  })
}

export function useUpdateReservationStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: {
      reservationId: string
      venueId: string
      status: ReservationStatus
      reason?: string
    }) => {
      const response = await api.patch<ApiResponse<Reservation>>(
        `/api/v1/reservations/${data.reservationId}/status`,
        {
          status: data.status,
          reason: data.reason,
        }
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['reservations', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['reservation', variables.reservationId] })
      queryClient.invalidateQueries({ queryKey: ['reservation-status-log', variables.reservationId] })
      toast.success('Rezervasyon durumu güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Durum güncellenemedi')
    },
  })
}

export function useEvaluateReservation() {
  return useMutation({
    mutationFn: async (data: {
      venueId: string
      tableId: string
      reservedFor: string
      partySize: number
      customerId?: string
    }) => {
      const response = await api.post<ApiResponse<ReservationEvaluationResult>>(
        `/api/v1/venues/${data.venueId}/reservations/evaluate`,
        data
      )
      return response.data.data
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Değerlendirme başarısız')
    },
  })
}

export function useSearchCustomers(venueId?: string, searchTerm?: string) {
  return useQuery({
    queryKey: ['customers', venueId, searchTerm],
    queryFn: async () => {
      if (!venueId || !searchTerm || searchTerm.length < 2) return []
      const response = await api.get<ApiResponse<Customer[]>>(
        `/api/v1/venues/${venueId}/customers/search?q=${encodeURIComponent(searchTerm)}`
      )
      return response.data.data
    },
    enabled: !!venueId && !!searchTerm && searchTerm.length >= 2,
  })
}

export function useExportReservations() {
  return useMutation({
    mutationFn: async (data: { venueId: string; date: string }) => {
      const response = await api.get(
        `/api/v1/venues/${data.venueId}/reservations/export?date=${data.date}`,
        { responseType: 'blob' }
      )
      
      // Create download
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `rezervasyonlar-${data.date}.csv`)
      document.body.appendChild(link)
      link.click()
      link.remove()
      window.URL.revokeObjectURL(url)
      
      toast.success('CSV indirildi')
    },
    onError: (error: any) => {
      toast.error('CSV indirilemedi')
    },
  })
}
