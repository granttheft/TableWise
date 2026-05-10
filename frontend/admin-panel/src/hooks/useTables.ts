import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { Table } from '@/types/api'

export function useTables(venueId?: string) {
  return useQuery({
    queryKey: ['tables', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<Table[]>(`/api/v1/table/venue/${venueId}`)
      return response.data
    },
    enabled: !!venueId,
  })
}

export function useCreateTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; table: Omit<Table, 'id' | 'createdAt' | 'updatedAt' | 'venueId' | 'displayOrder'> }) => {
      const response = await api.post<string>(
        `/api/v1/table/venue/${data.venueId}`,
        data.table
      )
      return response.data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Masa başarıyla oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa oluşturulamadı')
    },
  })
}

export function useUpdateTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableId: string; table: Partial<Table> }) => {
      await api.put(`/api/v1/table/${data.tableId}`, data.table)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      toast.success('Masa başarıyla güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa güncellenemedi')
    },
  })
}

export function useDeleteTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableId: string }) => {
      await api.delete(`/api/v1/table/${data.tableId}`)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Masa başarıyla silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa silinemedi')
    },
  })
}

export function useReorderTables() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableIds: string[] }) => {
      await api.post(`/api/v1/table/venue/${data.venueId}/reorder`, {
        tableIds: data.tableIds,
      })
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Sıralama güncellenemedi')
    },
  })
}
