import type { RuleType } from '@/types/api'

const API_TO_UI_RULE_TYPE: Record<string, RuleType> = {
  early_booking: 'EarlyBooking',
  vip_priority: 'VipPriority',
  large_group: 'LargeGroup',
  deposit_required: 'DepositRequired',
  peak_hour: 'PeakHour',
  table_cooldown: 'TableCooldown',
  group_composition: 'GroupComposition',
  custom_condition: 'CustomCondition',
  // Eski / alternatif adlar
  EarlyBooking: 'EarlyBooking',
  VipPriority: 'VipPriority',
  LargeGroup: 'LargeGroup',
  DepositRequired: 'DepositRequired',
  PeakHour: 'PeakHour',
  TableCooldown: 'TableCooldown',
  GroupComposition: 'GroupComposition',
  CustomCondition: 'CustomCondition',
}

/**
 * API veya veritabanından gelen kural tipi dizesini UI {@link RuleType} değerine çevirir.
 */
export function normalizeApiRuleType(ruleType: string | undefined | null): RuleType {
  if (!ruleType) return 'CustomCondition'
  const mapped = API_TO_UI_RULE_TYPE[ruleType]
  if (mapped) return mapped
  const snake = ruleType.replace(/([a-z])([A-Z])/g, '$1_$2').toLowerCase()
  return API_TO_UI_RULE_TYPE[snake] ?? 'CustomCondition'
}
