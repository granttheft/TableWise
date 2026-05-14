import type { Rule, RuleAction, RuleConditionGroup, RuleTrigger } from '@/types/api'
import { parseGroupCompositionConditionsJson } from './parseGroupCompositionConditions'
import { normalizeApiRuleType } from './normalizeRuleType'

/** GET /api/v1/rules yanıt satırı (camelCase serialization). */
export interface RuleApiDto {
  id: string
  venueId?: string | null
  name: string
  description?: string | null
  ruleType: string
  conditionsJson: string
  actionsJson: string
  priority: number
  triggerType: string
  isActive: boolean
  timesTriggered: number
  lastTriggeredAt?: string | null
  createdAt: string
  updatedAt?: string | null
}

function mapTriggerToUi(trigger: string): RuleTrigger {
  const t = trigger.trim()
  const m: Record<string, RuleTrigger> = {
    OnReservationCreate: 'OnReservation',
    OnSeatAssign: 'OnSeating',
    OnCancel: 'OnCancellation',
    OnReservation: 'OnReservation',
    OnSeating: 'OnSeating',
    OnModification: 'OnModification',
    OnCancellation: 'OnCancellation',
  }
  return m[t] ?? 'OnReservation'
}

function tryParseActions(actionsJson: string): RuleAction[] {
  if (!actionsJson?.trim()) return []
  try {
    const a = JSON.parse(actionsJson) as Record<string, unknown>
    if (Array.isArray(a.actions)) {
      return a.actions as RuleAction[]
    }
    if (typeof a.block === 'boolean' && a.block) {
      return [{ type: 'Block', message: typeof a.message === 'string' ? a.message : '' }]
    }
    if (typeof a.warn === 'boolean' && a.warn) {
      return [{ type: 'Warn', message: typeof a.message === 'string' ? a.message : '' }]
    }
    if (typeof a.discountPercent === 'number') {
      return [{ type: 'Discount', discountPercent: a.discountPercent, message: typeof a.message === 'string' ? a.message : undefined }]
    }
    if (typeof a.requireDeposit === 'boolean' && a.requireDeposit) {
      return [
        {
          type: 'Deposit',
          depositAmount: typeof a.depositAmount === 'number' ? a.depositAmount : 100,
          depositPerPerson: true,
        },
      ]
    }
  } catch {
    return []
  }
  return []
}

/**
 * API kural DTO'sunu admin panel {@link Rule} modeline dönüştürür.
 */
export function mapApiRuleDtoToRule(dto: RuleApiDto): Rule {
  const ruleType = normalizeApiRuleType(dto.ruleType)
  const emptyConditions: RuleConditionGroup = { conditions: [], logicalOperator: 'And' }

  const base: Rule = {
    id: dto.id,
    venueId: dto.venueId ?? '',
    name: dto.name,
    description: dto.description ?? undefined,
    ruleType,
    trigger: mapTriggerToUi(dto.triggerType),
    priority: dto.priority,
    isActive: dto.isActive,
    conditions: emptyConditions,
    actions: tryParseActions(dto.actionsJson),
    timesTriggered: dto.timesTriggered,
    lastTriggeredAt: dto.lastTriggeredAt ?? undefined,
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt ?? dto.createdAt,
  }

  if (ruleType === 'GroupComposition') {
    return {
      ...base,
      groupCompositionParams: parseGroupCompositionConditionsJson(dto.conditionsJson),
    }
  }

  return base
}
