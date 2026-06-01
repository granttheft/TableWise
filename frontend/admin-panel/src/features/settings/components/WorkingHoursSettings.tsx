import { useState, useEffect } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Switch } from '@/components/ui/switch'
import { Badge } from '@/components/ui/badge'
import { Calendar, Plus, X } from 'lucide-react'
import { toast } from 'sonner'
import { useVenues, useUpdateVenue } from '@/hooks/useVenues'

const daysOfWeek = [
  { value: 0, label: 'Pazar' },
  { value: 1, label: 'Pazartesi' },
  { value: 2, label: 'Salı' },
  { value: 3, label: 'Çarşamba' },
  { value: 4, label: 'Perşembe' },
  { value: 5, label: 'Cuma' },
  { value: 6, label: 'Cumartesi' },
]

interface DaySchedule {
  day: number
  isOpen: boolean
  openTime: string
  closeTime: string
}

export function WorkingHoursSettings() {
  const { data: venues = [] } = useVenues()
  const updateVenue = useUpdateVenue()
  const venue = venues[0]

  const [schedule, setSchedule] = useState<DaySchedule[]>(
    daysOfWeek.map((d) => ({
      day: d.value,
      isOpen: true,
      openTime: '10:00',
      closeTime: '23:00',
    }))
  )
  const [slotDuration, setSlotDuration] = useState('90')

  useEffect(() => {
    if (venue?.slotDurationMinutes) {
      setSlotDuration(String(venue.slotDurationMinutes))
    }
  }, [venue?.slotDurationMinutes])

  const [closures] = useState<{ id: string; date: string; reason: string; isPartial: boolean }[]>([])

  const toggleDay = (day: number) => {
    setSchedule(
      schedule.map((s) => (s.day === day ? { ...s, isOpen: !s.isOpen } : s))
    )
  }

  const updateSchedule = (day: number, field: 'openTime' | 'closeTime', value: string) => {
    setSchedule(schedule.map((s) => (s.day === day ? { ...s, [field]: value } : s)))
  }

  const handleSave = () => {
    if (!venue) {
      toast.error('Mekan bulunamadı')
      return
    }
    updateVenue.mutate({
      venueId: venue.id,
      updates: { slotDurationMinutes: Number(slotDuration) },
    })
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
                  <Switch
                    checked={daySchedule.isOpen}
                    onCheckedChange={() => toggleDay(day.value)}
                  />
                  <Label className="cursor-pointer" onClick={() => toggleDay(day.value)}>
                    {day.label}
                  </Label>
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
            <Button size="sm">
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
                <div
                  key={closure.id}
                  className="flex items-center justify-between rounded-lg border p-3"
                >
                  <div>
                    <div className="font-medium">{closure.date}</div>
                    <div className="text-sm text-muted-foreground">{closure.reason}</div>
                  </div>
                  <div className="flex items-center gap-2">
                    {closure.isPartial && <Badge variant="outline">Kısmi</Badge>}
                    <Button variant="ghost" size="icon">
                      <X className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
