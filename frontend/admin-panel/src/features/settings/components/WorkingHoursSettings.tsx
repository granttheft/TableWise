import { useState, useEffect } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Switch } from '@/components/ui/switch'
import { Badge } from '@/components/ui/badge'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Calendar, Plus, X } from 'lucide-react'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { toast } from 'sonner'
import { useVenues, useUpdateVenue } from '@/hooks/useVenues'
import { useVenueClosures, useCreateVenueClosure, useDeleteVenueClosure } from '@/hooks/useVenueClosures'

const daysOfWeek = [
  { value: 0, label: 'Pazar', key: 'Sunday' },
  { value: 1, label: 'Pazartesi', key: 'Monday' },
  { value: 2, label: 'Salı', key: 'Tuesday' },
  { value: 3, label: 'Çarşamba', key: 'Wednesday' },
  { value: 4, label: 'Perşembe', key: 'Thursday' },
  { value: 5, label: 'Cuma', key: 'Friday' },
  { value: 6, label: 'Cumartesi', key: 'Saturday' },
]

interface DaySchedule {
  day: number
  isOpen: boolean
  openTime: string
  closeTime: string
}

const defaultSchedule = (): DaySchedule[] =>
  daysOfWeek.map((d) => ({ day: d.value, isOpen: true, openTime: '10:00', closeTime: '23:00' }))

function parseWorkingHours(json: string | null | undefined): DaySchedule[] {
  if (!json) return defaultSchedule()
  try {
    const parsed = JSON.parse(json) as Record<string, { isClosed?: boolean; open?: string; close?: string }>
    return daysOfWeek.map((d) => {
      const entry = parsed[d.key]
      if (!entry || entry.isClosed) return { day: d.value, isOpen: false, openTime: '10:00', closeTime: '23:00' }
      return { day: d.value, isOpen: true, openTime: entry.open ?? '10:00', closeTime: entry.close ?? '23:00' }
    })
  } catch {
    return defaultSchedule()
  }
}

function buildWorkingHoursJson(schedule: DaySchedule[]): string {
  const obj: Record<string, unknown> = {}
  daysOfWeek.forEach((d) => {
    const s = schedule.find((x) => x.day === d.value)!
    obj[d.key] = s.isOpen ? { open: s.openTime, close: s.closeTime } : { isClosed: true }
  })
  return JSON.stringify(obj)
}

export function WorkingHoursSettings() {
  const { data: venues = [] } = useVenues()
  const updateVenue = useUpdateVenue()
  const venue = venues[0]

  const [schedule, setSchedule] = useState<DaySchedule[]>(defaultSchedule)
  const [slotDuration, setSlotDuration] = useState('90')
  const [closureModalOpen, setClosureModalOpen] = useState(false)
  const [closureDate, setClosureDate] = useState('')
  const [closureReason, setClosureReason] = useState('')

  const { data: closures = [] } = useVenueClosures(venue?.id)
  const createClosure = useCreateVenueClosure(venue?.id)
  const deleteClosure = useDeleteVenueClosure(venue?.id)

  useEffect(() => {
    if (!venue) return
    if (venue.slotDurationMinutes) setSlotDuration(String(venue.slotDurationMinutes))
    setSchedule(parseWorkingHours(venue.workingHours))
  }, [venue?.id, venue?.slotDurationMinutes, venue?.workingHours])

  const toggleDay = (day: number) =>
    setSchedule(schedule.map((s) => (s.day === day ? { ...s, isOpen: !s.isOpen } : s)))

  const updateSchedule = (day: number, field: 'openTime' | 'closeTime', value: string) =>
    setSchedule(schedule.map((s) => (s.day === day ? { ...s, [field]: value } : s)))

  const handleSave = () => {
    if (!venue) { toast.error('Mekan bulunamadı'); return }
    updateVenue.mutate(
      {
        venueId: venue.id,
        updates: {
          slotDurationMinutes: Number(slotDuration),
          workingHours: buildWorkingHoursJson(schedule),
        },
      },
      { onSuccess: () => toast.success('Çalışma saatleri kaydedildi') }
    )
  }

  const handleAddClosure = () => {
    if (!closureDate) { toast.error('Tarih seçiniz'); return }
    createClosure.mutate(
      { date: closureDate, reason: closureReason, isFullDay: true },
      {
        onSuccess: () => {
          setClosureModalOpen(false)
          setClosureDate('')
          setClosureReason('')
        },
      }
    )
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Rezervasyon Süresi</CardTitle>
          <CardDescription>Varsayılan rezervasyon slot süresi</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-4">
            <Label>Slot Süresi:</Label>
            <Select value={slotDuration} onValueChange={setSlotDuration}>
              <SelectTrigger className="w-[180px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="60">60 dakika</SelectItem>
                <SelectItem value="90">90 dakika</SelectItem>
                <SelectItem value="120">120 dakika</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Haftalık Çalışma Saatleri</CardTitle>
          <CardDescription>Her gün için açılış ve kapanış saatlerini ayarlayın</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {daysOfWeek.map((day) => {
            const daySchedule = schedule.find((s) => s.day === day.value)!
            return (
              <div key={day.value} className="flex items-center gap-4">
                <div className="flex w-32 items-center gap-2">
                  <Switch checked={daySchedule.isOpen} onCheckedChange={() => toggleDay(day.value)} />
                  <Label className="cursor-pointer" onClick={() => toggleDay(day.value)}>{day.label}</Label>
                </div>

                {daySchedule.isOpen ? (
                  <div className="flex items-center gap-2">
                    <Input
                      type="time"
                      value={daySchedule.openTime}
                      onChange={(e) => updateSchedule(day.value, 'openTime', e.target.value)}
                      className="w-32"
                    />
                    <span className="text-muted-foreground">-</span>
                    <Input
                      type="time"
                      value={daySchedule.closeTime}
                      onChange={(e) => updateSchedule(day.value, 'closeTime', e.target.value)}
                      className="w-32"
                    />
                  </div>
                ) : (
                  <Badge variant="secondary">Kapalı</Badge>
                )}
              </div>
            )
          })}

          <Button onClick={handleSave} className="mt-4" disabled={updateVenue.isPending}>
            {updateVenue.isPending ? 'Kaydediliyor...' : 'Kaydet'}
          </Button>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Kapalı Günler & Tatiller</CardTitle>
              <CardDescription>Özel kapalı günlerinizi işaretleyin</CardDescription>
            </div>
            <Button size="sm" onClick={() => setClosureModalOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Kapalı Gün Ekle
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {closures.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-8 text-center text-muted-foreground">
              <Calendar className="mb-2 h-12 w-12" />
              <p>Henüz kapalı gün eklenmedi</p>
              <p className="text-sm">Tatil günlerini ve özel kapalı günlerinizi buradan yönetin</p>
            </div>
          ) : (
            <div className="space-y-2">
              {closures.map((closure) => (
                <div key={closure.id} className="flex items-center justify-between rounded-lg border p-3">
                  <div>
                    <div className="font-medium">
                      {format(new Date(closure.date), 'd MMMM yyyy', { locale: tr })}
                    </div>
                    {closure.reason && (
                      <div className="text-sm text-muted-foreground">{closure.reason}</div>
                    )}
                  </div>
                  <div className="flex items-center gap-2">
                    {!closure.isFullDay && <Badge variant="outline">Kısmi</Badge>}
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => deleteClosure.mutate(closure.id)}
                      disabled={deleteClosure.isPending}
                    >
                      <X className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <Dialog open={closureModalOpen} onOpenChange={setClosureModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Kapalı Gün Ekle</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-2">
              <Label>Tarih</Label>
              <Input type="date" value={closureDate} onChange={(e) => setClosureDate(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Sebep (opsiyonel)</Label>
              <Input
                placeholder="Örn: Milli Bayram, Restorasyon..."
                value={closureReason}
                onChange={(e) => setClosureReason(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setClosureModalOpen(false)}>İptal</Button>
            <Button onClick={handleAddClosure} disabled={createClosure.isPending}>
              {createClosure.isPending ? 'Ekleniyor...' : 'Ekle'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
