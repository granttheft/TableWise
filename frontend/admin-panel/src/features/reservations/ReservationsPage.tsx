import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Badge } from '@/components/ui/badge'
import { Card } from '@/components/ui/card'
import { ChevronLeft, ChevronRight, Plus, Download, Calendar as CalendarIcon } from 'lucide-react'
import { format, addDays, subDays, startOfDay, parseISO, differenceInMinutes } from 'date-fns'
import { tr } from 'date-fns/locale'
import { useReservations, useExportReservations } from '@/hooks/useReservations'
import { useTables } from '@/hooks/useTables'
import { ReservationDetailDrawer } from './components/ReservationDetailDrawer'
import { ManualReservationDialog } from './components/ManualReservationDialog'
import { getStatusLabel, getStatusColor, calculateBlockPosition, generateTimeSlots } from './utils/reservationHelpers'
import type { ReservationStatus } from '@/types/api'

export function ReservationsPage() {
  const [selectedDate, setSelectedDate] = useState(startOfDay(new Date()))
  const [selectedVenue, setSelectedVenue] = useState<string>('all')
  const [selectedTable, setSelectedTable] = useState<string>('all')
  const [selectedStatus, setSelectedStatus] = useState<ReservationStatus | 'all'>('all')
  
  const [selectedReservationId, setSelectedReservationId] = useState<string | null>(null)
  const [isDetailDrawerOpen, setIsDetailDrawerOpen] = useState(false)
  const [isManualDialogOpen, setIsManualDialogOpen] = useState(false)
  const [presetSlot, setPresetSlot] = useState<{ tableId: string; timeSlot: string } | null>(null)

  const { data: reservations = [], isLoading } = useReservations({
    date: format(selectedDate, 'yyyy-MM-dd'),
    venueId: selectedVenue !== 'all' ? selectedVenue : undefined,
    tableId: selectedTable !== 'all' ? selectedTable : undefined,
    status: selectedStatus !== 'all' ? selectedStatus : undefined,
  })

  const { data: tables = [] } = useTables(selectedVenue !== 'all' ? selectedVenue : undefined)
  const exportReservations = useExportReservations()

  const timeSlots = generateTimeSlots(10, 24)

  const handlePrevDay = () => setSelectedDate(subDays(selectedDate, 1))
  const handleNextDay = () => setSelectedDate(addDays(selectedDate, 1))
  const handleToday = () => setSelectedDate(startOfDay(new Date()))

  const handleReservationClick = (reservationId: string) => {
    setSelectedReservationId(reservationId)
    setIsDetailDrawerOpen(true)
  }

  const handleSlotClick = (tableId: string, timeSlot: string) => {
    setPresetSlot({ tableId, timeSlot })
    setIsManualDialogOpen(true)
  }

  const handleExportCSV = async () => {
    try {
      await exportReservations.mutateAsync({
        date: format(selectedDate, 'yyyy-MM-dd'),
        venueId: selectedVenue !== 'all' ? selectedVenue : undefined,
      })
    } catch (error) {
      console.error('Export failed:', error)
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Rezervasyonlar</h1>
          <p className="text-muted-foreground">Günlük rezervasyon görünümü ve yönetimi</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleExportCSV}>
            <Download className="mr-2 h-4 w-4" />
            CSV İndir
          </Button>
          <Button onClick={() => setIsManualDialogOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Yeni Rezervasyon
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card className="p-4">
        <div className="flex flex-wrap items-center gap-4">
          {/* Date Navigation */}
          <div className="flex items-center gap-2">
            <Button variant="outline" size="icon" onClick={handlePrevDay}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button variant="outline" onClick={handleToday} className="min-w-[140px]">
              <CalendarIcon className="mr-2 h-4 w-4" />
              {format(selectedDate, 'dd MMMM yyyy', { locale: tr })}
            </Button>
            <Button variant="outline" size="icon" onClick={handleNextDay}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>

          {/* Venue Filter */}
          <Select value={selectedVenue} onValueChange={setSelectedVenue}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Tüm Mekanlar" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tüm Mekanlar</SelectItem>
              {/* TODO: Fetch venues */}
            </SelectContent>
          </Select>

          {/* Table Filter */}
          <Select value={selectedTable} onValueChange={setSelectedTable}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Tüm Masalar" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tüm Masalar</SelectItem>
              {tables?.map((table) => (
                <SelectItem key={table.id} value={table.id}>
                  {table.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {/* Status Filter */}
          <Select
            value={selectedStatus}
            onValueChange={(value) => setSelectedStatus(value as ReservationStatus | 'all')}
          >
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Tüm Durumlar" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tüm Durumlar</SelectItem>
              <SelectItem value="Confirmed">Onaylandı</SelectItem>
              <SelectItem value="Pending">Beklemede</SelectItem>
              <SelectItem value="Completed">Tamamlandı</SelectItem>
              <SelectItem value="Cancelled">İptal</SelectItem>
              <SelectItem value="NoShow">Gelmedi</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </Card>

      {/* Timeline View */}
      {isLoading ? (
        <div className="flex items-center justify-center py-12">
          <div className="text-muted-foreground">Yükleniyor...</div>
        </div>
      ) : !tables || tables.length === 0 ? (
        <Card className="flex flex-col items-center justify-center py-12">
          <p className="text-muted-foreground">Henüz masa eklenmemiş</p>
          <Button variant="link" asChild>
            <Link to="/tables">İlk masanızı ekleyin</Link>
          </Button>
        </Card>
      ) : (
        <div className="relative overflow-x-auto">
          <div className="min-w-[1200px]">
            {/* Time Header */}
            <div className="flex border-b sticky top-0 bg-background z-10">
              <div className="w-32 flex-shrink-0 border-r p-2 font-medium">Masa</div>
              <div className="flex flex-1">
                {timeSlots.map((slot) => (
                  <div key={slot} className="flex-1 border-r p-2 text-center text-sm text-muted-foreground">
                    {slot}
                  </div>
                ))}
              </div>
            </div>

            {/* Table Rows */}
            {tables.map((table) => {
              const tableReservations = reservations.filter((r) => r.tableId === table.id)
              
              return (
                <div key={table.id} className="flex border-b hover:bg-muted/50 transition-colors">
                  <div className="w-32 flex-shrink-0 border-r p-2">
                    <div className="font-medium">{table.name}</div>
                    <div className="text-sm text-muted-foreground">{table.capacity} kişi</div>
                  </div>
                  
                  <div className="flex flex-1 relative" style={{ minHeight: '80px' }}>
                    {timeSlots.map((slot) => (
                      <div
                        key={slot}
                        className="flex-1 border-r hover:bg-accent/10 cursor-pointer transition-colors"
                        onClick={() => handleSlotClick(table.id, slot)}
                      />
                    ))}

                    {/* Reservation Blocks */}
                    {tableReservations.map((reservation) => {
                      const durationMin = differenceInMinutes(
                        parseISO(reservation.endTime),
                        parseISO(reservation.reservedFor)
                      )
                      const { left, width } = calculateBlockPosition(
                        reservation.reservedFor,
                        durationMin,
                        '10:00'
                      )

                      return (
                        <button
                          key={reservation.id}
                          onClick={(e) => {
                            e.stopPropagation()
                            handleReservationClick(reservation.id)
                          }}
                          className={`absolute top-2 bottom-2 rounded px-2 py-1 text-xs text-white hover:opacity-90 transition-opacity ${getStatusColor(
                            reservation.status as ReservationStatus
                          )}`}
                          style={{
                            left: `${left}%`,
                            width: `${width}%`,
                          }}
                        >
                          <div className="font-medium truncate">{reservation.guestName}</div>
                          <div className="text-xs opacity-90">{reservation.partySize} kişi</div>
                        </button>
                      )
                    })}
                  </div>
                </div>
              )
            })}
          </div>
        </div>
      )}

      {/* Mobile List View (Responsive) */}
      <div className="lg:hidden">
        <Card>
          {reservations.length > 0 ? (
            <div className="divide-y">
              {reservations.map((reservation) => (
                  <button
                    key={reservation.id}
                    onClick={() => handleReservationClick(reservation.id)}
                    className="w-full p-4 text-left hover:bg-muted transition-colors"
                  >
                    <div className="flex items-start justify-between">
                      <div>
                        <div className="font-medium">{reservation.guestName}</div>
                        <div className="text-sm text-muted-foreground">
                          {format(parseISO(reservation.reservedFor), 'HH:mm')} • {reservation.partySize} kişi
                        </div>
                      </div>
                      <Badge variant="secondary" className={getStatusColor(reservation.status as ReservationStatus)}>
                        {getStatusLabel(reservation.status as ReservationStatus)}
                      </Badge>
                    </div>
                  </button>
                ))}
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <p className="text-muted-foreground">Bu tarihte rezervasyon yok</p>
            </div>
          )}
        </Card>
      </div>

      {/* Detail Drawer */}
      {selectedReservationId && (
        <ReservationDetailDrawer
          open={isDetailDrawerOpen}
          onOpenChange={setIsDetailDrawerOpen}
          reservationId={selectedReservationId}
        />
      )}

      {/* Manual Reservation Dialog */}
      <ManualReservationDialog
        open={isManualDialogOpen}
        onOpenChange={setIsManualDialogOpen}
        venueId={selectedVenue !== 'all' ? selectedVenue : ''}
        tables={tables || []}
        presetTableId={presetSlot?.tableId}
        presetTimeSlot={presetSlot?.timeSlot}
        presetDate={format(selectedDate, 'yyyy-MM-dd')}
      />
    </div>
  )
}
