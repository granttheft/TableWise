/** API ve kural motoru ile uyumlu grup kompozisyonu kodları. */
export type GroupCompositionBookingValue = 'Mixed' | 'AllMale' | 'AllFemale' | 'Family'

/** Rezervasyon formu: seçim veya bilinçli olarak boş. */
export type GroupCompositionBookingSelection = GroupCompositionBookingValue | ''

export interface VenueCustomFieldBase {
  field_key: string
  label: string
  required?: boolean
}
