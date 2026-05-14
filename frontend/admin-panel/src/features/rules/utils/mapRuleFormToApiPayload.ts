import type { RuleAction, RuleCondition, RuleConditionGroup } from '@/types/api'

/** Maps UI ruleType (PascalCase) to API / evaluator snake_case. */
const UI_RULE_TYPE_TO_API: Record<string, string> = {
  VipPriority: 'vip_priority',
  EarlyBooking: 'early_booking',
  LargeGroup: 'large_group',
  DepositRequired: 'deposit_required',
  PeakHour: 'peak_hour',
  TableCooldown: 'table_cooldown',
  GroupComposition: 'group_composition',
  CustomCondition: 'custom_condition',
}

const DAY_INDEX_TO_ENGLISH = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
] as const

const CONDITION_OP_TO_API: Record<string, string> = {
  Equals: '==',
  NotEquals: '!=',
  GreaterThan: '>',
  GreaterThanOrEquals: '>=',
  LessThan: '<',
  LessThanOrEquals: '<=',
  In: 'in',
  Contains: 'contains',
  Between: '==',
}

function toApiRuleType(ui: string): string {
  return UI_RULE_TYPE_TO_API[ui] ?? 'custom_condition'
}

function mapTriggerToApi(trigger: string): string {
  const m: Record<string, string> = {
    OnReservation: 'OnReservationCreate',
    OnSeating: 'OnSeatAssign',
    OnModification: 'OnReservationCreate',
    OnCancellation: 'OnCancel',
  }
  return m[trigger] ?? 'OnReservationCreate'
}

function normalizeCustomerTier(value: string): string {
  const v = value.trim()
  if (/^vip$/i.test(v)) return 'VIP'
  if (/^gold$/i.test(v)) return 'Gold'
  if (/^regular$/i.test(v)) return 'Regular'
  if (/^blacklisted$/i.test(v)) return 'Blacklisted'
  return v
}

function mapUiFieldToApiField(field: string): string | null {
  const f = field.trim()
  switch (f) {
    case 'PartySize':
      return 'partySize'
    case 'DaysInAdvance':
      return 'daysInAdvance'
    case 'Hour':
      return 'reservation.reservedFor.hour'
    case 'DayOfWeek':
      return 'reservation.reservedFor.dayOfWeek'
    case 'CustomerTier':
      return 'customer.tier'
    case 'OccupancyPercent':
      return 'venue.currentOccupancy'
    case 'HasChildren':
    case 'ChildCount':
    case 'AdultCount':
    case 'MinutesSinceLastReservation':
    case 'IsWeekend':
    case 'IsHoliday':
      return null
    default:
      return null
  }
}

function mapDayValueToEnglish(value: unknown): string | null {
  if (typeof value === 'number' && value >= 0 && value <= 6) {
    return DAY_INDEX_TO_ENGLISH[value] ?? null
  }
  if (typeof value === 'string') {
    const n = parseInt(value, 10)
    if (!Number.isNaN(n) && n >= 0 && n <= 6) return DAY_INDEX_TO_ENGLISH[n] ?? null
    return value
  }
  return null
}

function normalizeConditionValue(
  apiField: string,
  op: string,
  raw: string | number | boolean | string[] | number[]
): unknown {
  if (apiField === 'venue.currentOccupancy') {
    const n = typeof raw === 'number' ? raw : parseFloat(String(raw))
    if (!Number.isNaN(n) && n > 1) return n / 100
    return n
  }
  if (apiField === 'customer.tier') {
    if (op === 'in' && Array.isArray(raw)) {
      return (raw as unknown[]).map((x) => normalizeCustomerTier(String(x)))
    }
    return normalizeCustomerTier(String(raw))
  }
  if (apiField === 'reservation.reservedFor.dayOfWeek') {
    if (op === 'in' && Array.isArray(raw)) {
      return (raw as unknown[])
        .map((x) => (typeof x === 'number' || typeof x === 'string' ? mapDayValueToEnglish(x) : null))
        .filter((x): x is string => x != null)
    }
    return mapDayValueToEnglish(raw) ?? String(raw)
  }
  return raw
}

function buildCustomConditionsJson(group: RuleConditionGroup): string {
  const op = (group.logicalOperator ?? 'And').toLowerCase() === 'or' ? 'or' : 'and'
  const conditions = (group.conditions ?? []).map((c) => {
    const apiField = mapUiFieldToApiField(c.field)
    if (!apiField) {
      throw new Error(`Desteklenmeyen alan: ${c.field}`)
    }
    const apiOp = CONDITION_OP_TO_API[c.operator] ?? '=='
    const value = normalizeConditionValue(apiField, apiOp, c.value)
    return { field: apiField, op: apiOp, value }
  })

  return JSON.stringify({
    version: 1,
    operator: op,
    conditions,
  })
}

function buildCustomActionsJson(actions: RuleAction[]): string {
  const a = actions[0]
  const block = a?.type === 'Block'
  const warn = a?.type === 'Warn'
  const suggest = a?.type === 'Suggest'
  const discountPercent =
    a?.type === 'Discount' && a.discountPercent != null ? Number(a.discountPercent) : null
  const requireDeposit = a?.type === 'Deposit'
  const depositAmount =
    a?.type === 'Deposit' && a.depositAmount != null ? Number(a.depositAmount) : null

  return JSON.stringify({
    version: 1,
    block,
    warn,
    discountPercent: discountPercent != null && discountPercent > 0 ? discountPercent : null,
    suggest,
    requireDeposit,
    depositAmount,
    message: a?.message ?? '',
  })
}

function firstNumberCondition(conds: RuleCondition[], field: string, ops: string[]): number | null {
  const c = conds.find((x) => x.field === field && ops.includes(x.operator))
  if (!c) return null
  const n = typeof c.value === 'number' ? c.value : parseFloat(String(c.value))
  return Number.isNaN(n) ? null : n
}

function minTierFromVipTemplate(conds: RuleCondition[]): string {
  const c = conds.find((x) => x.field === 'CustomerTier')
  if (!c) return 'Gold'
  if (c.operator === 'Equals' || c.operator === 'NotEquals') {
    return normalizeCustomerTier(String(c.value))
  }
  if (c.operator === 'In' && Array.isArray(c.value)) {
    const tiers = (c.value as unknown[]).map((v) => normalizeCustomerTier(String(v)))
    if (tiers.includes('VIP')) return 'VIP'
    if (tiers.includes('Gold')) return 'Gold'
    return tiers[0] ?? 'Gold'
  }
  return 'Gold'
}

function buildTypedPayload(
  apiRuleType: string,
  group: RuleConditionGroup,
  actions: RuleAction[]
): { conditionsJson: string; actionsJson: string } {
  const conds = group.conditions ?? []

  switch (apiRuleType) {
    case 'vip_priority': {
      const minTier = minTierFromVipTemplate(conds)
      return {
        conditionsJson: JSON.stringify({ version: 1, minTier }),
        actionsJson: JSON.stringify({ version: 1, suggestBestTable: true }),
      }
    }
    case 'early_booking': {
      const minDays =
        firstNumberCondition(conds, 'DaysInAdvance', ['GreaterThanOrEquals', 'GreaterThan']) ?? 7
      const discount = actions.find((x) => x.type === 'Discount')
      const discountPercent =
        discount?.discountPercent != null ? Number(discount.discountPercent) : 10
      const label = discount?.message ?? actions[0]?.message ?? 'Erken rezervasyon indirimi uygulandı'
      return {
        conditionsJson: JSON.stringify({ version: 1, minDaysInAdvance: Math.floor(minDays) }),
        actionsJson: JSON.stringify({
          version: 1,
          discountPercent,
          label,
        }),
      }
    }
    case 'large_group': {
      const minParty =
        firstNumberCondition(conds, 'PartySize', ['GreaterThanOrEquals', 'GreaterThan']) ?? 8
      const msg =
        actions.find((a) => a.type === 'Warn' || a.type === 'Block')?.message ??
        'Büyük grup - lütfen telefonla teyit alın'
      return {
        conditionsJson: JSON.stringify({ version: 1, minPartySize: Math.floor(minParty) }),
        actionsJson: JSON.stringify({
          version: 1,
          requireCombination: false,
          message: msg,
        }),
      }
    }
    case 'deposit_required': {
      const minParty =
        firstNumberCondition(conds, 'PartySize', ['GreaterThanOrEquals', 'GreaterThan']) ?? 6
      const dep = actions.find((x) => x.type === 'Deposit')
      const amount = dep?.depositAmount != null ? Number(dep.depositAmount) : 100
      const perPerson = dep?.depositPerPerson === true
      const useVenueDefault = dep?.useVenueDefault === true
      return {
        conditionsJson: JSON.stringify({
          version: 1,
          scopes: { minPartySize: Math.floor(minParty) },
        }),
        actionsJson: JSON.stringify({
          version: 1,
          amount,
          perPerson,
          useVenueDefault,
        }),
      }
    }
    case 'peak_hour': {
      let startH = firstNumberCondition(conds, 'Hour', ['GreaterThanOrEquals', 'GreaterThan']) ?? 19
      const gt = conds.find((c) => c.field === 'Hour' && c.operator === 'GreaterThan')
      if (gt && typeof gt.value === 'number') startH = gt.value + 1

      let endInclusive =
        firstNumberCondition(conds, 'Hour', ['LessThanOrEquals', 'LessThan']) ?? 22
      const lt = conds.find((c) => c.field === 'Hour' && c.operator === 'LessThan')
      if (lt && typeof lt.value === 'number') endInclusive = lt.value - 1

      const startTime = `${String(Math.max(0, Math.min(23, Math.floor(startH)))).padStart(2, '0')}:00`
      const endBoundary = Math.min(23, Math.floor(endInclusive)) + 1
      const endTime = `${String(endBoundary).padStart(2, '0')}:00`

      const dayCond = conds.find((c) => c.field === 'DayOfWeek' && c.operator === 'In')
      let days: string[] | undefined
      if (dayCond && Array.isArray(dayCond.value)) {
        days = (dayCond.value as unknown[])
          .map((x) => mapDayValueToEnglish(x))
          .filter((x): x is string => x != null)
        if (days.length === 0) days = undefined
      }

      const act = actions[0]
      const block = act?.type === 'Block'
      const warn = act?.type === 'Warn' || act?.type === 'Suggest' || !block

      return {
        conditionsJson: JSON.stringify({
          version: 1,
          startTime,
          endTime,
          minOccupancyPercent: null,
          days: days ?? null,
        }),
        actionsJson: JSON.stringify({
          version: 1,
          block,
          warn: warn && !block,
          message: act?.message ?? 'Bu saat dilimi yoğun dönemdir.',
        }),
      }
    }
    case 'table_cooldown': {
      const lt = firstNumberCondition(conds, 'MinutesSinceLastReservation', ['LessThan', 'LessThanOrEquals'])
      const minutes = lt != null ? Math.floor(lt) : 30
      const msg =
        actions.find((a) => a.type === 'Block')?.message ??
        `Bu masa için en az ${minutes} dakika bekleme süresi gerekli`
      return {
        conditionsJson: JSON.stringify({ version: 1, cooldownMinutes: minutes }),
        actionsJson: JSON.stringify({ version: 1, message: msg }),
      }
    }
    case 'group_composition': {
      const msg = actions[0]?.message ?? 'Grup kompozisyonu kuralı'
      return {
        conditionsJson: JSON.stringify({ version: 1, operator: 'or', rules: [] }),
        actionsJson: JSON.stringify({
          version: 1,
          block: false,
          warn: true,
          message: msg,
        }),
      }
    }
    default:
      return {
        conditionsJson: buildCustomConditionsJson(group),
        actionsJson: buildCustomActionsJson(actions),
      }
  }
}

export type RuleBuilderSubmitPayload = {
  /** Present on dialog submit; ignored when mapping to API DTO. */
  venueId?: string
  name: string
  description?: string
  priority: number
  trigger: string
  isActive: boolean
  ruleType: string
  conditions: RuleConditionGroup
  actions: RuleAction[]
}

/**
 * Converts rule builder form values into the JSON shape expected by POST/PUT `/api/v1/rules`.
 */
export function mapRuleFormToApiPayload(payload: RuleBuilderSubmitPayload): {
  name: string
  description: string | null
  ruleType: string
  conditionsJson: string
  actionsJson: string
  priority: number
  triggerType: string
  isActive: boolean
} {
  const apiRuleType = toApiRuleType(payload.ruleType)
  const group: RuleConditionGroup = {
    conditions: payload.conditions.conditions ?? [],
    logicalOperator: payload.conditions.logicalOperator ?? 'And',
  }

  let conditionsJson: string
  let actionsJson: string

  if (apiRuleType === 'custom_condition') {
    conditionsJson = buildCustomConditionsJson(group)
    actionsJson = buildCustomActionsJson(payload.actions)
  } else {
    const typed = buildTypedPayload(apiRuleType, group, payload.actions)
    conditionsJson = typed.conditionsJson
    actionsJson = typed.actionsJson
  }

  const desc = payload.description?.trim()
  return {
    name: payload.name.trim(),
    description: desc ? desc : null,
    ruleType: apiRuleType,
    conditionsJson,
    actionsJson,
    priority: payload.priority,
    triggerType: mapTriggerToApi(payload.trigger),
    isActive: payload.isActive,
  }
}
