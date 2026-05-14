import type {
  Rule,
  RuleCondition,
  RuleAction,
  ConditionOperator,
  RuleType,
  GroupCompositionRuleFormState,
} from '@/types/api'
import { dayLabels } from '../data/ruleTemplates'
import {
  GROUP_COMPOSITION_LABELS,
  type GroupCompositionValue,
} from '../constants/groupCompositionRule'

/**
 * Koşul operatörünü Türkçe metne çevirir
 */
function getOperatorText(operator: ConditionOperator, value: any): string {
  switch (operator) {
    case 'Equals':
      return `${value} olduğunda`
    case 'NotEquals':
      return `${value} olmadığında`
    case 'GreaterThan':
      return `${value}'den fazla olduğunda`
    case 'GreaterThanOrEquals':
      return `${value} veya daha fazla olduğunda`
    case 'LessThan':
      return `${value}'den az olduğunda`
    case 'LessThanOrEquals':
      return `${value} veya daha az olduğunda`
    case 'Contains':
      return `${value} içerdiğinde`
    case 'In':
      return `${Array.isArray(value) ? value.join(', ') : value} içinde olduğunda`
    case 'Between':
      return `arasında olduğunda`
    default:
      return `${value}`
  }
}

/**
 * Alan adını Türkçe metne çevirir
 */
function getFieldLabel(field: string): string {
  const fieldLabels: Record<string, string> = {
    PartySize: 'Kişi sayısı',
    DaysInAdvance: 'Rezervasyon günü',
    Hour: 'Saat',
    DayOfWeek: 'Gün',
    CustomerTier: 'Müşteri tipi',
    OccupancyPercent: 'Doluluk oranı',
    HasChildren: 'Çocuklu grup',
    ChildCount: 'Çocuk sayısı',
    AdultCount: 'Yetişkin sayısı',
    MinutesSinceLastReservation: 'Son rezervasyondan geçen süre',
    IsWeekend: 'Hafta sonu',
    IsHoliday: 'Tatil günü',
  }
  return fieldLabels[field] || field
}

/**
 * Müşteri tipini Türkçe metne çevirir
 */
function getCustomerTierLabel(tier: string): string {
  const labels: Record<string, string> = {
    Regular: 'Normal müşteri',
    Vip: 'VIP müşteri',
    Blacklisted: 'Kara listedeki müşteri',
  }
  return labels[tier] || tier
}

/**
 * Günleri Türkçe metne çevirir
 */
function getDaysText(days: number[]): string {
  if (days.length === 7) return 'her gün'
  if (days.length === 0) return ''
  
  const weekdays = [1, 2, 3, 4, 5]
  const weekend = [0, 6]
  
  if (weekdays.every(d => days.includes(d)) && days.length === 5) {
    return 'hafta içi'
  }
  if (weekend.every(d => days.includes(d)) && days.length === 2) {
    return 'hafta sonu'
  }
  
  return days.map(d => dayLabels[d]).join(', ')
}

/**
 * Tek bir koşulu Türkçe metne çevirir
 */
function conditionToText(condition: RuleCondition): string {
  const field = getFieldLabel(condition.field)
  
  // Özel durumlar
  if (condition.field === 'CustomerTier') {
    const tierLabel = getCustomerTierLabel(condition.value as string)
    if (condition.operator === 'Equals') {
      return tierLabel
    }
    return `${tierLabel} ${condition.operator === 'NotEquals' ? 'olmadığında' : ''}`
  }
  
  if (condition.field === 'DayOfWeek' && condition.operator === 'In') {
    const raw = condition.value
    const dayNums = Array.isArray(raw)
      ? raw.map((v) => (typeof v === 'number' ? v : parseInt(String(v), 10)))
      : []
    const daysText = getDaysText(dayNums)
    return `${daysText} günlerinde`
  }
  
  if (condition.field === 'HasChildren' || condition.field === 'IsWeekend' || condition.field === 'IsHoliday') {
    return condition.value ? field : `${field} değil`
  }
  
  if (condition.field === 'DaysInAdvance') {
    if (condition.operator === 'GreaterThanOrEquals') {
      return `${condition.value}+ gün öncesinden yapıldığında`
    }
    if (condition.operator === 'LessThanOrEquals') {
      return `${condition.value} gün veya daha az önceden yapıldığında`
    }
  }
  
  if (condition.field === 'Hour') {
    return `saat ${condition.value}${condition.operator === 'GreaterThanOrEquals' ? ' ve sonrasında' : condition.operator === 'LessThanOrEquals' ? ' ve öncesinde' : ':00\'da'}`
  }
  
  if (condition.field === 'PartySize') {
    if (condition.operator === 'GreaterThanOrEquals') {
      return `${condition.value}+ kişilik gruplar için`
    }
    if (condition.operator === 'LessThanOrEquals') {
      return `${condition.value} kişi veya daha az gruplar için`
    }
    return `${condition.value} kişilik gruplar için`
  }
  
  if (condition.field === 'OccupancyPercent') {
    return `doluluk %${condition.value}${condition.operator === 'GreaterThanOrEquals' ? ' ve üzerinde' : condition.operator === 'LessThanOrEquals' ? ' ve altında' : ''} olduğunda`
  }
  
  return `${field} ${getOperatorText(condition.operator, condition.value)}`
}

/**
 * Aksiyonu Türkçe metne çevirir
 */
function actionToText(action: RuleAction): string {
  switch (action.type) {
    case 'Block':
      return 'rezervasyon engellenir'
    case 'Warn':
      return `uyarı gösterilir${action.message ? `: "${action.message}"` : ''}`
    case 'Suggest':
      return `öneri yapılır${action.message ? `: "${action.message}"` : ''}`
    case 'Discount':
      return `%${action.discountPercent} indirim uygulanır`
    case 'Deposit':
      if (action.depositPerPerson) {
        return `kişi başı ₺${action.depositAmount} kapora istenir`
      }
      return `₺${action.depositAmount} kapora istenir`
    case 'Redirect':
      return `${action.redirectUrl || 'başka sayfaya'} yönlendirilir`
    default:
      return action.type
  }
}

/**
 * Grup kompozisyonu form durumunu ve aksiyonları okunabilir Türkçe özet metne çevirir.
 */
export function groupCompositionToHumanReadable(
  state: GroupCompositionRuleFormState,
  actions: RuleAction[]
): string {
  const hasBlocked = state.blockedCompositions.length > 0
  const femaleMin = state.minFemaleRatioPercent
  const hasFemaleMin = femaleMin != null && femaleMin > 0
  const maleMax = state.maxMaleRatioPercent
  const hasMaleMax = maleMax != null && maleMax < 100
  const minParty = Math.max(1, Math.floor(state.minPartySize || 1))

  const blockedHuman = state.blockedCompositions
    .map((b) => GROUP_COMPOSITION_LABELS[b as GroupCompositionValue] ?? b)
    .join(', ')

  const onlyAllMale =
    state.blockedCompositions.length === 1 &&
    state.blockedCompositions[0]?.toLowerCase() === 'allmale'

  const blockedShort = onlyAllMale ? 'Sadece erkek gruplar' : `${blockedHuman} grupları`

  let conditionPart = ''

  if (hasBlocked && hasFemaleMin) {
    conditionPart = `${blockedShort} ve %${femaleMin} altı kadın oranı engellenir`
  } else if (hasBlocked && hasMaleMax && !hasFemaleMin) {
    conditionPart = `${blockedShort}; erkek oranı üst sınırı aşıldığında kısıt uygulanır`
  } else if (hasBlocked) {
    if (onlyAllMale && minParty >= 2) {
      conditionPart = `${minParty} ve üzeri kişilik sadece erkek gruplar engellenir`
    } else {
      conditionPart = `${blockedHuman || 'Seçilen'} kompozisyonları engellenir`
    }
  }

  if (!hasBlocked && hasFemaleMin && hasMaleMax) {
    conditionPart = `Grupta en az %${femaleMin} kadın misafir bulunmalıdır; en fazla %${maleMax} erkek oranına izin verilir`
  } else if (!hasBlocked && hasFemaleMin) {
    conditionPart = `Grupta en az %${femaleMin} kadın misafir bulunmalıdır`
  } else if (!hasBlocked && hasMaleMax) {
    conditionPart = `Grupta en fazla %${maleMax} erkek oranına izin verilir`
  }

  if (!conditionPart) {
    conditionPart = 'Grup kompozisyonu kuralı tanımlı'
  }

  const act = actions[0]
  let actionPart = 'işlem uygulanmaz'
  if (act?.type === 'Block') actionPart = 'rezervasyon engellenir'
  else if (act?.type === 'Warn') actionPart = `uyarı gösterilir${act.message ? `: "${act.message}"` : ''}`
  else if (act?.type === 'Suggest') actionPart = `öneri yapılır${act.message ? `: "${act.message}"` : ''}`

  const opNote =
    state.operator === 'or'
      ? ' Alt koşullardan biri tetiklenir (VEYA).'
      : ' Alt koşullar birlikte değerlendirilir (VE).'

  return `${conditionPart}${opNote} ${actionPart}.`
}

/**
 * Kuralı tam Türkçe açıklamaya çevirir
 */
export function ruleToHumanReadable(rule: Partial<Rule> & { ruleType?: RuleType }): string {
  if (rule.ruleType === 'GroupComposition' && rule.groupCompositionParams) {
    return groupCompositionToHumanReadable(rule.groupCompositionParams, rule.actions ?? [])
  }

  const { conditions, actions } = rule

  if (!conditions?.conditions?.length && !actions?.length) {
    return 'Koşul ve aksiyon tanımlanmamış'
  }

  // Koşulları birleştir
  const conditionTexts = conditions?.conditions?.map(conditionToText) || []
  const conditionPart = conditionTexts.length > 0
    ? conditionTexts.join(conditions!.logicalOperator === 'And' ? ' ve ' : ' veya ')
    : 'Her durumda'

  // Aksiyonları birleştir
  const actionTexts = actions?.map(actionToText) || []
  const actionPart = actionTexts.length > 0 ? actionTexts.join(' ve ') : 'hiçbir işlem yapılmaz'

  return `${conditionPart}, ${actionPart}.`
}

/**
 * Kural tipini Türkçe başlığa çevirir
 */
export function getRuleTypeLabel(ruleType: string): string {
  const labels: Record<string, string> = {
    early_booking: 'Erken Rezervasyon',
    vip_priority: 'VIP Önceliği',
    large_group: 'Büyük Grup',
    deposit_required: 'Kapora Zorunluluğu',
    peak_hour: 'Yoğun Saat',
    table_cooldown: 'Masa Arası Süre',
    group_composition: 'Grup Dengesi',
    custom_condition: 'Özel Kural',
    EarlyBooking: 'Erken Rezervasyon',
    VipPriority: 'VIP Önceliği',
    LargeGroup: 'Büyük Grup',
    DepositRequired: 'Kapora Zorunluluğu',
    PeakHour: 'Yoğun Saat',
    TableCooldown: 'Masa Arası Süre',
    GroupComposition: 'Grup Dengesi',
    CustomCondition: 'Özel Kural',
  }
  return labels[ruleType] || ruleType
}

/**
 * Trigger tipini Türkçe etikete çevirir
 */
export function getTriggerLabel(trigger: string): string {
  const labels: Record<string, string> = {
    OnReservation: 'Rezervasyonda',
    OnSeating: 'Oturmada',
    OnModification: 'Değişiklikte',
    OnCancellation: 'İptalde',
  }
  return labels[trigger] || trigger
}

/**
 * Öncelik seviyesine göre renk sınıfı döndürür
 */
export function getPriorityColor(priority: number): string {
  if (priority >= 8) return 'bg-red-500 text-white'
  if (priority >= 5) return 'bg-amber-500 text-white'
  if (priority >= 3) return 'bg-blue-500 text-white'
  return 'bg-gray-500 text-white'
}

/**
 * Öncelik seviyesine göre açıklama döndürür
 */
export function getPriorityDescription(priority: number): string {
  if (priority >= 9) return 'Kritik - Her zaman ilk değerlendirilir'
  if (priority >= 7) return 'Yüksek - Önemli iş kuralları için'
  if (priority >= 5) return 'Normal - Standart kurallar için'
  if (priority >= 3) return 'Düşük - Opsiyonel öneriler için'
  return 'Çok düşük - Nadir durumlar için'
}
