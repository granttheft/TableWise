import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { Table, ApiResponse } from '@/types/api'

export function useTables(venueId?: string) {
  return useQuery({
    queryKey: ['tables', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<ApiResponse<Table[]>>(`/api/v1/venues/${venueId}/tables`)
      return response.data.data
    },
    enabled: !!venueId,
  })
}

export function useCreateTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; table: Omit<Table, 'id' | 'createdAt' | 'updatedAt' | 'venueId' | 'displayOrder'> }) => {
      const response = await api.post<ApiResponse<Table>>(
        `/api/v1/venues/${data.venueId}/tables`,
        data.table
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Masa başarıyla oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Masa oluşturulamadı')
    },
  })
}

export function useUpdateTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableId: string; table: Partial<Table> }) => {
      const response = await api.put<ApiResponse<Table>>(
        `/api/v1/venues/${data.venueId}/tables/${data.tableId}`,
        data.table
      )
      return response.data.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      toast.success('Masa başarıyla güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Masa güncellenemedi')
    },
  })
}

export function useDeleteTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableId: string }) => {
      await api.delete(`/api/v1/venues/${data.venueId}/tables/${data.tableId}`)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Masa başarıyla silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Masa silinemedi')
    },
  })
}

export function useReorderTables() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableIds: string[] }) => {
      await api.post(`/api/v1/venues/${data.venueId}/tables/reorder`, {
        tableIds: data.tableIds,
      })
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Sıralama güncellenemedi')
    },
  })
}
