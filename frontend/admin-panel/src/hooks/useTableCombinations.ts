import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { TableCombination } from '@/types/api'

/**
 * Venue'ye ait masa birleşimlerini getirir
 */
export function useTableCombinations(venueId?: string) {
  return useQuery({
    queryKey: ['table-combinations', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<TableCombination[]>(
        `/api/v1/table-combination/venue/${venueId}`
      )
      return response.data
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
      const response = await api.post<string>(
        `/api/v1/table-combination/venue/${data.venueId}`,
        {
          name: data.name,
          tableIds: data.tableIds,
          combinedCapacity: data.combinedCapacity,
        }
      )
      return response.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['table-combinations', variables.venueId] })
      toast.success('Masa birleşimi oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa birleşimi oluşturulamadı')
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
      await api.delete(`/api/v1/table-combination/${data.combinationId}`)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['table-combinations', variables.venueId] })
      toast.success('Masa birleşimi silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa birleşimi silinemedi')
    },
  })
}
