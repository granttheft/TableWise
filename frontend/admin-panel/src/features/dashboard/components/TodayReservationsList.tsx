import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Users, Clock } from 'lucide-react'
import { format, parseISO } from 'date-fns'
import type { TodayReservation } from '@/types/api'

interface TodayReservationsListProps {
  data?: TodayReservation[]
  isLoading?: boolean
}

const statusMap: Record<string, { label: string; variant: 'default' | 'secondary' | 'destructive' | 'outline' }> = {
  Pending: { label: 'Bekliyor', variant: 'outline' },
  Confirmed: { label: 'Onaylandı', variant: 'default' },
  Modified: { label: 'Değiştirildi', variant: 'secondary' },
  Completed: { label: 'Tamamlandı', variant: 'secondary' },
  Cancelled: { label: 'İptal', variant: 'destructive' },
  NoShow: { label: 'Gelmedi', variant: 'destructive' },
}

export function TodayReservationsList({ data, isLoading }: TodayReservationsListProps) {
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>
        <CardContent className="space-y-4">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="flex items-center space-x-4">
              <Skeleton className="h-12 w-12 rounded" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-3 w-3/4" />
              </div>
            </div>
          ))}
        </CardContent>
      </Card>
    )
  }

  if (!data || data.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Bugünkü Rezervasyonlar</CardTitle>
          <CardDescription>Günlük rezervasyon listesi</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <Clock className="mb-4 h-12 w-12 text-muted-foreground" />
            <h3 className="mb-2 text-lg font-semibold">Henüz rezervasyon yok</h3>
            <p className="mb-4 text-sm text-muted-foreground">
              Booking linkinizi paylaşarak rezervasyon almaya başlayın!
            </p>
            <button
              onClick={() => {
                const bookingUrl = `${import.meta.env.VITE_BOOKING_BASE_URL}/demo-slug`
                navigator.clipboard.writeText(bookingUrl)
              }}
              className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground hover:bg-accent/90"
            >
              Booking URL'yi Kopyala
            </button>
          </div>
        </CardContent>
      </Card>
    )
  }

  // Group by hour
  const groupedByHour = data.reduce((acc, reservation) => {
    const hour = format(parseISO(reservation.reservedFor), 'HH:00')
    if (!acc[hour]) {
      acc[hour] = []
    }
    acc[hour].push(reservation)
    return acc
  }, {} as Record<string, TodayReservation[]>)

  const sortedHours = Object.keys(groupedByHour).sort()

  return (
    <Card>
      <CardHeader>
        <CardTitle>Bugünkü Rezervasyonlar</CardTitle>
        <CardDescription>{data.length} rezervasyon</CardDescription>
      </CardHeader>
      <CardContent className="max-h-[400px] overflow-y-auto space-y-4">
        {sortedHours.map((hour) => (
          <div key={hour} className="space-y-2">
            <div className="text-sm font-medium text-muted-foreground">{hour}</div>
            {groupedByHour[hour].map((reservation) => (
              <div
                key={reservation.id}
                className="flex items-center justify-between rounded-lg border p-3 hover:bg-accent/50 cursor-pointer transition-colors"
                onClick={() => {
                  // TODO: Open drawer with reservation details
                  console.log('Open reservation', reservation.id)
                }}
              >
                <div className="flex-1">
                  <div className="flex items-center space-x-2">
                    <span className="font-medium">{reservation.guestName}</span>
                    <Badge variant={statusMap[reservation.status]?.variant || 'outline'}>
                      {statusMap[reservation.status]?.label || reservation.status}
                    </Badge>
                  </div>
                  <div className="mt-1 flex items-center space-x-3 text-xs text-muted-foreground">
                    <span className="flex items-center">
                      <Users className="mr-1 h-3 w-3" />
                      {reservation.partySize} kişi
                    </span>
                    <span>Masa: {reservation.tableName}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ))}
      </CardContent>
    </Card>
  )
}
