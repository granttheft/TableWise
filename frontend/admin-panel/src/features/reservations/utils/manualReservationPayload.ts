import type { ManualReservationForm } from '../components/ManualReservationDialog'

/**
 * Form tarih + saat alanlarını API ReservedFor ISO değerine çevirir.
 */
export function buildReservedForIso(date: string, timeSlot: string): string {
  const time = timeSlot.length === 5 ? `${timeSlot}:00` : timeSlot
  const local = new Date(`${date}T${time}`)
  if (Number.isNaN(local.getTime())) {
    return `${date}T${time}`
  }
  return local.toISOString()
}

/**
 * Manuel rezervasyon formunu CreateReservationDto gövdesine map eder.
 */
export function mapManualReservationToCreatePayload(data: ManualReservationForm) {
  return {
    venueId: data.venueId,
    tableId: data.tableId,
    guestName: data.guestName,
    guestEmail: data.guestEmail || null,
    guestPhone: data.guestPhone || '0000000000',
    partySize: data.guestCount,
    reservedFor: buildReservedForIso(data.date, data.timeSlot),
    specialRequests: data.specialRequest || null,
    bypassDeposit: false,
    bypassRules: data.overrideRules ?? false,
    sendConfirmationEmail: true,
  }
}

/**
 * Kural kontrolü isteği gövdesi.
 */
export function mapManualReservationToEvaluatePayload(data: {
  venueId: string
  tableId: string
  date: string
  timeSlot: string
  guestCount: number
  customerId?: string
  guestEmail?: string
  guestPhone?: string
}) {
  return {
    venueId: data.venueId,
    tableId: data.tableId,
    partySize: data.guestCount,
    reservedFor: buildReservedForIso(data.date, data.timeSlot),
    customerId: data.customerId || null,
    guestEmail: data.guestEmail || null,
    guestPhone: data.guestPhone || null,
  }
}
