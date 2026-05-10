import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import { format } from 'date-fns'
import type { TodayReservation } from '@/types/api'

/** API liste satırı (ReservationDto ile uyumlu alanlar). */
interface ReservationListItem {
  id: string
  guestName: string
  guestPhone: string
  partySize: number
  reservedFor: string
  tableName?: string | null
  tableCombinationName?: string | null
  status: string
}

interface ReservationListResponse {
  items: ReservationListItem[]
  totalCount: number
  page: number
  pageSize: number
}

/**
 * Bugünkü rezervasyonlar (UTC takvim günü, fromDate/toDate).
 */
export function useTodayReservations() {
  return useQuery({
    queryKey: ['reservations', 'today'],
    queryFn: async () => {
      const day = format(new Date(), 'yyyy-MM-dd')
      const fromDate = `${day}T00:00:00.000Z`
      const toDate = `${day}T23:59:59.999Z`
      const { data } = await api.get<ReservationListResponse>('/api/v1/reservations', {
        params: {
          fromDate,
          toDate,
          pageSize: 100,
          page: 1,
        },
      })

      return (data.items ?? []).map(
        (r): TodayReservation => ({
          id: r.id,
          guestName: r.guestName,
          guestPhone: r.guestPhone ?? '',
          partySize: r.partySize,
          reservedFor: r.reservedFor,
          tableName: r.tableName ?? r.tableCombinationName ?? '—',
          status: r.status as TodayReservation['status'],
        })
      )
    },
    refetchInterval: 30000,
  })
}
