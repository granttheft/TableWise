import type { RuleTestContext, RuleTestResult } from '@/types/api'

const DAY_INDEX_TO_ENGLISH = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
] as const

/** API test isteği gövdesi (TestRuleRequestDto). */
export interface ApiTestRuleContext {
  partySize: number
  daysInAdvance: number
  customerTier?: string
  dayOfWeek?: string
  hour: number
  venueOccupancy: number
  maleCount?: number
  femaleCount?: number
  groupComposition?: string
}

interface ApiRuleTestResultDto {
  triggered: boolean
  executionMs: number
  outcome?: {
    ruleId: string
    ruleName: string
    actionType: string
    message?: string
    payload?: string
  }
  conditionEvaluations?: {
    field: string
    op: string
    expectedValue?: unknown
    actualValue?: unknown
    result: boolean
  }[]
}

/**
 * UI test bağlamını API TestRuleRequestDto formatına çevirir.
 */
export function mapTestContextToApi(context: RuleTestContext): ApiTestRuleContext {
  const day =
    context.dayOfWeek >= 0 && context.dayOfWeek <= 6
      ? DAY_INDEX_TO_ENGLISH[context.dayOfWeek]
      : undefined

  return {
    partySize: context.partySize,
    daysInAdvance: context.daysInAdvance,
    customerTier: context.customerTier,
    dayOfWeek: day,
    hour: context.hour,
    venueOccupancy: Math.min(1, Math.max(0, context.occupancyPercent / 100)),
    maleCount: context.hasChildren ? context.childCount : undefined,
    femaleCount: undefined,
  }
}

/**
 * API test yanıtını UI {@link RuleTestResult} modeline dönüştürür.
 */
export function mapRuleTestResultFromApi(dto: ApiRuleTestResultDto): RuleTestResult {
  return {
    triggered: dto.triggered,
    executionTimeMs: dto.executionMs,
    outcome: dto.outcome
      ? {
          action: dto.outcome.actionType,
          message: dto.outcome.message,
          payload: dto.outcome.payload as Record<string, unknown> | string | undefined,
        }
      : undefined,
    conditionEvaluations: dto.conditionEvaluations,
  }
}
