import type { GroupCompositionRuleFormState } from '@/types/api'

/** Grup kompozisyonu API değerleri (Mixed, AllMale, AllFemale, Family). */
export const GROUP_COMPOSITION_VALUES = ['Mixed', 'AllMale', 'AllFemale', 'Family'] as const

export type GroupCompositionValue = (typeof GROUP_COMPOSITION_VALUES)[number]

/** Kompozisyon kodu → Türkçe kısa etiket (form ve önizleme). */
export const GROUP_COMPOSITION_LABELS: Record<GroupCompositionValue, string> = {
  Mixed: 'Karma',
  AllMale: 'Sadece erkek',
  AllFemale: 'Sadece kadın',
  Family: 'Aile',
}

/** Şablondan gelen varsayılan form durumu (conditionsJson ile uyumlu). */
export const DEFAULT_GROUP_COMPOSITION_FORM: GroupCompositionRuleFormState = {
  operator: 'and',
  blockedCompositions: ['AllMale'],
  minPartySize: 4,
  minFemaleRatioPercent: null,
  maxMaleRatioPercent: null,
}

/** Grup dengesi kuralı vurgu rengi (şablon kartı, liste, chip). */
export const GROUP_COMPOSITION_ACCENT_HEX = '#ec4899'
