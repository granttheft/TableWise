/** API custom_condition koşul alanı (backend whitelist ile uyumlu). */
export interface CustomConditionFieldRef {
  field: string
  label: string
  description: string
  exampleValue: string
  operators: string[]
}

/** İzin verilen operatörler (API formatı). */
export const CUSTOM_CONDITION_OPERATORS = ['==', '!=', '<', '<=', '>', '>=', 'in', 'contains'] as const

/**
 * Özel kural JSON editörü için alan referans tablosu.
 * Backend {@link RuleSchemaValidator.AllowedFields} ile senkron tutulmalı.
 */
export const CUSTOM_CONDITION_FIELD_REFERENCE: CustomConditionFieldRef[] = [
  {
    field: 'partySize',
    label: 'Kişi sayısı',
    description: 'Rezervasyon partisi kişi adedi.',
    exampleValue: '6',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'daysInAdvance',
    label: 'Kaç gün öncesi',
    description: 'Rezervasyonun kaç gün önceden yapıldığı.',
    exampleValue: '7',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'customer.tier',
    label: 'Müşteri seviyesi',
    description: 'CRM tier: Regular, Gold, VIP, Blacklisted.',
    exampleValue: '"VIP"',
    operators: ['==', '!=', 'in'],
  },
  {
    field: 'customer.totalVisits',
    label: 'Toplam ziyaret',
    description: 'Müşterinin toplam ziyaret sayısı.',
    exampleValue: '10',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'reservation.reservedFor.hour',
    label: 'Rezervasyon saati',
    description: '0–23 arası saat.',
    exampleValue: '20',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'reservation.reservedFor.dayOfWeek',
    label: 'Haftanın günü',
    description: 'İngilizce gün adı veya in operatörü ile dizi.',
    exampleValue: '"Saturday"',
    operators: ['==', '!=', 'in'],
  },
  {
    field: 'venue.currentOccupancy',
    label: 'Mekan doluluğu',
    description: '0.0–1.0 arası oran (örn. %90 → 0.9).',
    exampleValue: '0.85',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'table.capacity',
    label: 'Masa kapasitesi',
    description: 'Seçilen masanın kapasitesi.',
    exampleValue: '4',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'table.location',
    label: 'Masa konumu',
    description: 'Masa lokasyon kodu veya metni.',
    exampleValue: '"Outdoor"',
    operators: ['==', '!=', 'in', 'contains'],
  },
  {
    field: 'groupComposition',
    label: 'Grup kompozisyonu',
    description: 'Mixed, AllMale, AllFemale, Family.',
    exampleValue: '"AllMale"',
    operators: ['==', '!=', 'in'],
  },
  {
    field: 'femaleRatio',
    label: 'Kadın oranı',
    description: '0.0–1.0 arası (örn. %30 → 0.3).',
    exampleValue: '0.3',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'maleRatio',
    label: 'Erkek oranı',
    description: '0.0–1.0 arası.',
    exampleValue: '0.7',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'maleCount',
    label: 'Erkek sayısı',
    description: 'Gruptaki erkek misafir sayısı.',
    exampleValue: '4',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
  {
    field: 'femaleCount',
    label: 'Kadın sayısı',
    description: 'Gruptaki kadın misafir sayısı.',
    exampleValue: '2',
    operators: ['==', '!=', '<', '<=', '>', '>='],
  },
]

export const CUSTOM_CONDITION_FIELD_SET = new Set(
  CUSTOM_CONDITION_FIELD_REFERENCE.map((f) => f.field.toLowerCase())
)

export const CUSTOM_CONDITION_OPERATOR_SET = new Set(
  CUSTOM_CONDITION_OPERATORS.map((o) => o.toLowerCase())
)
