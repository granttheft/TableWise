import type { ReservationStatus } from '@/types/api'
import { format, parseISO, addMinutes } from 'date-fns'
import { tr } from 'date-fns/locale'

/**
 * Rezervasyon durumunu Türkçe etikete çevirir
 */
export function getStatusLabel(status: ReservationStatus): string {
  const labels: Record<ReservationStatus, string> = {
    Pending: 'Bekliyor',
    Confirmed: 'Onaylandı',
    Seated: 'Oturdu',
    Completed: 'Tamamlandı',
    Cancelled: 'İptal',
    NoShow: 'Gelmedi',
  }
  return labels[status]
}

/**
 * Rezervasyon durumuna göre renk sınıfı döndürür
 */
export function getStatusColor(status: ReservationStatus): string {
  const colors: Record<ReservationStatus, string> = {
    Pending: 'bg-amber-500',
    Confirmed: 'bg-emerald-500',
    Seated: 'bg-blue-500',
    Completed: 'bg-slate-500',
    Cancelled: 'bg-gray-400 opacity-50',
    NoShow: 'bg-rose-500',
  }
  return colors[status]
}

/**
 * Rezervasyon durumuna göre badge variant döndürür
 */
export function getStatusVariant(status: ReservationStatus): 'default' | 'secondary' | 'destructive' | 'outline' {
  const variants: Record<ReservationStatus, 'default' | 'secondary' | 'destructive' | 'outline'> = {
    Pending: 'outline',
    Confirmed: 'default',
    Seated: 'secondary',
    Completed: 'secondary',
    Cancelled: 'destructive',
    NoShow: 'destructive',
  }
  return variants[status]
}

/**
 * Saati 30 dakikalık bloklara yuvarlar
 */
export function roundToNearestSlot(date: Date): Date {
  const minutes = date.getMinutes()
  const roundedMinutes = Math.round(minutes / 30) * 30
  date.setMinutes(roundedMinutes)
  date.setSeconds(0)
  date.setMilliseconds(0)
  return date
}

/**
 * Saat dizisi oluşturur (örn: ['12:00', '12:30', '13:00', ...])
 */
export function generateTimeSlots(startHour: number = 10, endHour: number = 24): string[] {
  const slots: string[] = []
  for (let hour = startHour; hour < endHour; hour++) {
    slots.push(`${hour.toString().padStart(2, '0')}:00`)
    slots.push(`${hour.toString().padStart(2, '0')}:30`)
  }
  return slots
}

/**
 * Rezervasyon bloğunun timeline'daki pozisyonunu hesaplar
 */
export function calculateBlockPosition(
  reservedFor: string,
  durationMinutes: number,
  dayStart: string = '10:00'
): { left: string; width: string } {
  const [dayStartHour, dayStartMinute] = dayStart.split(':').map(Number)
  const dayStartMinutes = dayStartHour * 60 + dayStartMinute

  const reservationDate = parseISO(reservedFor)
  const reservationMinutes = reservationDate.getHours() * 60 + reservationDate.getMinutes()

  const offsetMinutes = reservationMinutes - dayStartMinutes
  const totalDayMinutes = 14 * 60 // 14 hours (10:00 - 24:00)

  const left = (offsetMinutes / totalDayMinutes) * 100
  const width = (durationMinutes / totalDayMinutes) * 100

  return {
    left: `${Math.max(0, left)}%`,
    width: `${Math.min(100 - left, width)}%`,
  }
}

/**
 * Müşteri tier'ını Türkçe etikete çevirir
 */
export function getTierLabel(tier: 'Regular' | 'Vip' | 'Blacklisted'): string {
  const labels = {
    Regular: 'Normal',
    Vip: 'VIP',
    Blacklisted: 'Kara Liste',
  }
  return labels[tier]
}

/**
 * Müşteri tier'ına göre renk sınıfı döndürür
 */
export function getTierColor(tier: 'Regular' | 'Vip' | 'Blacklisted'): string {
  const colors = {
    Regular: 'bg-gray-500',
    Vip: 'bg-amber-500',
    Blacklisted: 'bg-red-500',
  }
  return colors[tier]
}

/**
 * Tarih formatlar (Türkçe)
 */
export function formatDate(date: string | Date, formatStr: string = 'PPP'): string {
  const dateObj = typeof date === 'string' ? parseISO(date) : date
  return format(dateObj, formatStr, { locale: tr })
}

/**
 * Saat formatlar
 */
export function formatTime(date: string | Date): string {
  const dateObj = typeof date === 'string' ? parseISO(date) : date
  return format(dateObj, 'HH:mm')
}

/**
 * Bitiş saatini hesaplar
 */
export function calculateEndTime(startTime: string, durationMinutes: number): string {
  const startDate = parseISO(startTime)
  const endDate = addMinutes(startDate, durationMinutes)
  return format(endDate, 'HH:mm')
}

/**
 * Confirm code formatlar (4-4)
 */
export function formatConfirmCode(code: string): string {
  if (code.length !== 8) return code
  return `${code.slice(0, 4)}-${code.slice(4)}`
}
