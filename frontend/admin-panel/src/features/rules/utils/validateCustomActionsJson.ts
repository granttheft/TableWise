import type { CustomConditionValidationIssue } from './validateCustomConditionJson'

export interface CustomActionsValidationResult {
  valid: boolean
  issues: CustomConditionValidationIssue[]
}

const MAX_DISCOUNT_PERCENT = 50

/**
 * custom_condition actionsJson metnini doğrular.
 */
export function validateCustomActionsJson(jsonText: string): CustomActionsValidationResult {
  const issues: CustomConditionValidationIssue[] = []
  const trimmed = jsonText.trim()

  if (!trimmed) {
    return {
      valid: false,
      issues: [{ path: '$', message: 'Aksiyon JSON boş olamaz', line: 1 }],
    }
  }

  let parsed: Record<string, unknown>
  try {
    parsed = JSON.parse(trimmed) as Record<string, unknown>
  } catch (e) {
    const msg = e instanceof Error ? e.message : 'Geçersiz JSON'
    return { valid: false, issues: [{ path: '$', message: msg, line: 1 }] }
  }

  if (parsed.version !== 1) {
    issues.push({ path: 'version', message: 'version alanı zorunludur ve 1 olmalıdır' })
  }

  const block = parsed.block === true
  const warn = parsed.warn === true
  const suggest = parsed.suggest === true
  const requireDeposit = parsed.requireDeposit === true
  const discount =
    typeof parsed.discountPercent === 'number' ? parsed.discountPercent : null

  if (!block && !warn && !suggest && !requireDeposit && (discount == null || discount <= 0)) {
    issues.push({
      path: '$',
      message: 'En az bir aksiyon seçilmeli (block, warn, suggest, requireDeposit veya discountPercent)',
    })
  }

  if (discount != null) {
    if (discount < 0 || discount > MAX_DISCOUNT_PERCENT) {
      issues.push({
        path: 'discountPercent',
        message: `İndirim oranı 0–${MAX_DISCOUNT_PERCENT} arasında olmalıdır`,
      })
    }
  }

  if (requireDeposit) {
    const amount = parsed.depositAmount
    if (amount != null && (typeof amount !== 'number' || amount < 0)) {
      issues.push({ path: 'depositAmount', message: 'depositAmount geçerli bir sayı olmalıdır' })
    }
  }

  if (typeof parsed.message !== 'string') {
    issues.push({ path: 'message', message: 'message metin (string) olmalıdır' })
  }

  return { valid: issues.length === 0, issues }
}
