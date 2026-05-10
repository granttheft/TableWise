import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import { format } from 'date-fns'
import type { TodayReservation, ApiResponse } from '@/types/api'

export function useTodayReservations() {
  return useQuery({
    queryKey: ['reservations', 'today'],
    queryFn: async () => {
      const today = format(new Date(), 'yyyy-MM-dd')
      const response = await api.get<ApiResponse<TodayReservation[]>>(
        `/api/v1/reservations?date=${today}`
      )
      return response.data.data
    },
    refetchInterval: 30000, // 30 seconds
  })
}
