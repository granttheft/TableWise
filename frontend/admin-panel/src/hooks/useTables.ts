import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import type { Table } from '@/types/api'

const tablesBase = (venueId: string) => `/api/v1/venues/${venueId}/tables`

/**
 * Venue'ye ait masaları getirir.
 */
export function useTables(venueId?: string) {
  return useQuery({
    queryKey: ['tables', venueId],
    queryFn: async () => {
      if (!venueId) return []
      const response = await api.get<Table[]>(tablesBase(venueId))
      return response.data
    },
    enabled: !!venueId,
  })
}

type CreateTablePayload = Pick<Table, 'name' | 'capacity' | 'location' | 'description'>

/**
 * Yeni masa oluşturur (CreateTableDto: name, capacity, location, description).
 */
export function useCreateTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; table: CreateTablePayload & { isActive?: boolean } }) => {
      const { isActive: _ignored, ...body } = data.table
      const response = await api.post<string>(tablesBase(data.venueId), body)
      return response.data
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      void queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Masa başarıyla oluşturuldu')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa oluşturulamadı')
    },
  })
}

/**
 * Masa bilgilerini günceller (UpdateTableDto).
 */
export function useUpdateTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableId: string; table: Partial<Table> }) => {
      const t = data.table
      await api.put(`${tablesBase(data.venueId)}/${data.tableId}`, {
        name: t.name,
        capacity: t.capacity,
        location: t.location,
        description: t.description ?? null,
      })
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      toast.success('Masa başarıyla güncellendi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa güncellenemedi')
    },
  })
}

/**
 * Masayı siler (soft delete).
 */
export function useDeleteTable() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableId: string }) => {
      await api.delete(`${tablesBase(data.venueId)}/${data.tableId}`)
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
      void queryClient.invalidateQueries({ queryKey: ['plan-limits'] })
      toast.success('Masa başarıyla silindi')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa silinemedi')
    },
  })
}

/**
 * Masa aktif/pasif sunucu tarafında toggle edilir.
 */
export function useToggleTableActive() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableId: string }) => {
      await api.put(`${tablesBase(data.venueId)}/${data.tableId}/toggle`)
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Masa durumu güncellenemedi')
    },
  })
}

/**
 * Masa sıralamasını günceller (ReorderTablesDto: items).
 */
export function useReorderTables() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { venueId: string; tableIds: string[] }) => {
      const items = data.tableIds.map((id, index) => ({
        id,
        sortOrder: index,
      }))
      await api.put(`${tablesBase(data.venueId)}/reorder`, { items })
    },
    onSuccess: (_, variables) => {
      void queryClient.invalidateQueries({ queryKey: ['tables', variables.venueId] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Sıralama güncellenemedi')
    },
  })
}
