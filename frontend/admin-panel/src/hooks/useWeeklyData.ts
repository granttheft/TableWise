import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { WeeklyReservationData, ApiResponse } from '@/types/api'

export function useWeeklyData() {
  return useQuery({
    queryKey: ['dashboard', 'weekly'],
    queryFn: async () => {
      const response = await api.get<ApiResponse<WeeklyReservationData[]>>(
        '/api/v1/dashboard/weekly-data'
      )
      return response.data.data
    },
    refetchInterval: 60000, // 60 seconds
  })
}
