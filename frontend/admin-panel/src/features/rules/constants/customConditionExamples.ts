export interface CustomConditionExample {
  id: string
  label: string
  conditionsJson: string
  actionsJson: string
}

/** Örnek yükle dropdown seçenekleri. */
export const CUSTOM_CONDITION_EXAMPLES: CustomConditionExample[] = [
  {
    id: 'large-male-group',
    label: 'Büyük Erkek Grubu',
    conditionsJson: JSON.stringify(
      {
        version: 1,
        operator: 'and',
        conditions: [
          { field: 'partySize', op: '>=', value: 6 },
          { field: 'groupComposition', op: '==', value: 'AllMale' },
        ],
      },
      null,
      2
    ),
    actionsJson: JSON.stringify(
      {
        version: 1,
        block: true,
        warn: false,
        message: '6 ve üzeri sadece erkek gruplar için rezervasyon kabul edilmemektedir.',
      },
      null,
      2
    ),
  },
  {
    id: 'vip-weekend',
    label: 'VIP Hafta Sonu',
    conditionsJson: JSON.stringify(
      {
        version: 1,
        operator: 'and',
        conditions: [
          { field: 'customer.tier', op: '==', value: 'VIP' },
          {
            field: 'reservation.reservedFor.dayOfWeek',
            op: 'in',
            value: ['Saturday', 'Sunday'],
          },
        ],
      },
      null,
      2
    ),
    actionsJson: JSON.stringify(
      {
        version: 1,
        warn: true,
        suggest: true,
        message: 'VIP misafirlerimize hafta sonu öncelikli masa ayrılmıştır.',
      },
      null,
      2
    ),
  },
  {
    id: 'peak-hour',
    label: 'Yoğun Saat Kısıtı',
    conditionsJson: JSON.stringify(
      {
        version: 1,
        operator: 'and',
        conditions: [
          { field: 'reservation.reservedFor.hour', op: '>=', value: 19 },
          { field: 'reservation.reservedFor.hour', op: '<=', value: 22 },
          { field: 'venue.currentOccupancy', op: '>=', value: 0.8 },
        ],
      },
      null,
      2
    ),
    actionsJson: JSON.stringify(
      {
        version: 1,
        block: true,
        message: 'Yoğun saatlerde yeni rezervasyon alınamıyor.',
      },
      null,
      2
    ),
  },
  {
    id: 'min-female-ratio',
    label: 'Minimum Kadın Oranı',
    conditionsJson: JSON.stringify(
      {
        version: 1,
        operator: 'and',
        conditions: [
          { field: 'partySize', op: '>=', value: 4 },
          { field: 'femaleRatio', op: '<', value: 0.3 },
        ],
      },
      null,
      2
    ),
    actionsJson: JSON.stringify(
      {
        version: 1,
        block: true,
        message: 'Grupta en az %30 kadın misafir bulunmalıdır.',
      },
      null,
      2
    ),
  },
]

export const DEFAULT_CUSTOM_CONDITIONS_JSON = JSON.stringify(
  {
    version: 1,
    operator: 'and',
    conditions: [{ field: 'partySize', op: '>=', value: 4 }],
  },
  null,
  2
)

export const DEFAULT_CUSTOM_ACTIONS_JSON = JSON.stringify(
  {
    version: 1,
    block: false,
    warn: true,
    message: '',
  },
  null,
  2
)
