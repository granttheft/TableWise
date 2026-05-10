import { useQuery, useMutation, useQueryClient } from '@tantml:function_calls>
import api from '@/lib/api'
import { toast } from 'sonner'
import type { 
  Reservation, 
  ReservationStatus, 
  ReservationStatusLog,
  ReservationEvaluationResult,
  Customer
} from '@/types/api'

interface ReservationsFilters {
  date?: string
  venueId?: string
  tableId?: string
  status?: ReservationStatus | 'all'
}

export function useReservations(filters?: ReservationsFilters) {
  return useQuery({
    queryKey: ['reservations', filters],
    queryFn: async () => {
      const params = new URLSearchParams()
      
      if (filters?.date) params.append('date', filters.date)
      if (filters?.venueId) params.append('venueId', filters.venueId)
      if (filters?.tableId && filters.tableId !== 'all') params.append('tableId', filters.tableId)
      if (filters?.status && filters.status !== 'all') params.append('status', filters.status)

      const response = await api.get<ApiResponse<Reservation[]>>(
        `/api/v1/reservations?${params.toString()}`
      )
      return response.data.data
    },
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
    mutationFn: async (data: any) => {
      const response = await api.post<ApiResponse<Reservation>>(
        '/api/v1/reservations',
        data
      )
      return response.data.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
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
      updates: Partial<Reservation>
    }) => {
      const response = await api.put<ApiResponse<Reservation>>(
        `/api/v1/reservations/${data.reservationId}`,
        data.updates
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
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
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
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
      date: string
      timeSlot: string
      guestCount: number
      customerId?: string
    }) => {
      const response = await api.post<ApiResponse<ReservationEvaluationResult>>(
        `/api/v1/reservations/evaluate`,
        data
      )
      return response.data.data
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Değerlendirme başarısız')
    },
  })
}

export function useSearchCustomers(searchTerm?: string, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ['customers-search', searchTerm],
    queryFn: async () => {
      if (!searchTerm || searchTerm.length < 2) return []
      const response = await api.get<Customer[]>(
        `/api/v1/customer/search?q=${encodeURIComponent(searchTerm)}`
      )
      return response.data
    },
    enabled: options?.enabled !== false && !!searchTerm && searchTerm.length >= 2,
  })
}

export function useExportReservations() {
  return useMutation({
    mutationFn: async (data: { date: string; venueId?: string }) => {
      const params = new URLSearchParams()
      params.append('date', data.date)
      if (data.venueId) params.append('venueId', data.venueId)

      const response = await api.get(
        `/api/v1/reservations/export?${params.toString()}`,
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
