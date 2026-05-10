import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { TableCombination, ApiResponse } from '@/types/api'

/**
 * Venue'ye ait masa birleşimlerini getirir
 */
export function useTableCombinations(venueId?: string) {
  return useQuery({
    queryKey: ['table-combinations', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<ApiResponse<TableCombination[]>>(
        `/api/v1/venues/${venueId}/table-combinations`
      )
      return response.data.data
    },
    enabled: !!venueId,
  })
}

/**
 * Yeni masa birleşimi oluşturur
 */
export function useCreateTableCombination() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: {
      venueId: string
      name: string
      tableIds: string[]
      combinedCapacity: number
    }) => {
      const response = await api.post<ApiResponse<TableCombination>>(
        `/api/v1/venues/${data.venueId}/table-combinations`,
        {
          name: data.name,
          tableIds: data.tableIds,
          combinedCapacity: data.combinedCapacity,
        }
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['table-combinations', variables.venueId] })
      toast.success('Masa birleşimi oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Masa birleşimi oluşturulamadı')
    },
  })
}

/**
 * Masa birleşimini siler
 */
export function useDeleteTableCombination() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; combinationId: string }) => {
      await api.delete(`/api/v1/table-combinations/${data.combinationId}`)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['table-combinations', variables.venueId] })
      toast.success('Masa birleşimi silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Masa birleşimi silinemedi')
    },
  })
}
