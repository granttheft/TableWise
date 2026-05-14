import type { GroupCompositionRuleFormState } from '@/types/api'
import { DEFAULT_GROUP_COMPOSITION_FORM } from '../constants/groupCompositionRule'

interface CompositionRuleJson {
  type?: string
  blockedCompositions?: string[]
  allowedCompositions?: string[]
  minPartySize?: number
  minFemaleRatio?: number
  maxMaleRatio?: number
}

interface GroupCompositionConditionsJson {
  version?: number
  operator?: string
  rules?: CompositionRuleJson[]
}

/**
 * group_composition conditionsJson içeriğini düzenleme formu durumuna çevirir.
 */
export function parseGroupCompositionConditionsJson(
  conditionsJson: string | undefined | null
): GroupCompositionRuleFormState {
  if (!conditionsJson?.trim()) {
    return { ...DEFAULT_GROUP_COMPOSITION_FORM }
  }
  try {
    const raw = JSON.parse(conditionsJson) as GroupCompositionConditionsJson
    const op = raw.operator?.toLowerCase() === 'or' ? 'or' : 'and'
    let blocked: string[] = []
    let minPartySize = DEFAULT_GROUP_COMPOSITION_FORM.minPartySize
    let minFemaleRatioPercent: number | null = null
    let maxMaleRatioPercent: number | null = null

    for (const rule of raw.rules ?? []) {
      const t = rule.type?.toLowerCase()
      if (t === 'composition' && Array.isArray(rule.blockedCompositions)) {
        blocked = [...rule.blockedCompositions]
      }
      if (t === 'ratio') {
        if (typeof rule.minPartySize === 'number' && !Number.isNaN(rule.minPartySize)) {
          minPartySize = Math.floor(rule.minPartySize)
        }
        if (typeof rule.minFemaleRatio === 'number' && !Number.isNaN(rule.minFemaleRatio)) {
          minFemaleRatioPercent = Math.round(rule.minFemaleRatio * 100)
        }
        if (typeof rule.maxMaleRatio === 'number' && !Number.isNaN(rule.maxMaleRatio)) {
          maxMaleRatioPercent = Math.round(rule.maxMaleRatio * 100)
        }
      }
    }

    return {
      operator: op,
      blockedCompositions: blocked,
      minPartySize,
      minFemaleRatioPercent,
      maxMaleRatioPercent,
    }
  } catch {
    return { ...DEFAULT_GROUP_COMPOSITION_FORM }
  }
}
