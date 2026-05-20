import {
  CUSTOM_CONDITION_FIELD_SET,
  CUSTOM_CONDITION_OPERATOR_SET,
} from '../constants/customConditionFieldReference'

export interface CustomConditionValidationIssue {
  path: string
  message: string
  /** JSON metninde yaklaşık satır (1 tabanlı). */
  line?: number
}

export interface CustomConditionValidationResult {
  valid: boolean
  issues: CustomConditionValidationIssue[]
}

function findLineForToken(json: string, token: string): number | undefined {
  const idx = json.indexOf(token)
  if (idx < 0) return undefined
  return json.slice(0, idx).split('\n').length
}

function isValidConditionValue(field: string, op: string, value: unknown): string | null {
  if (value === undefined || value === null) {
    return 'value alanı zorunludur'
  }
  const f = field.toLowerCase()
  if (f === 'customer.tier') {
    const tiers = ['Regular', 'Gold', 'VIP', 'Blacklisted']
    if (op === 'in' && Array.isArray(value)) {
      for (const v of value) {
        if (typeof v !== 'string' || !tiers.includes(v)) {
          return `Geçersiz tier: ${String(v)}`
        }
      }
      return null
    }
    if (typeof value === 'string' && !tiers.includes(value)) {
      return `Geçersiz tier: ${value}`
    }
  }
  if (f === 'groupcomposition') {
    const comps = ['Mixed', 'AllMale', 'AllFemale', 'Family']
    if (op === 'in' && Array.isArray(value)) {
      for (const v of value) {
        if (typeof v !== 'string' || !comps.includes(v)) {
          return `Geçersiz kompozisyon: ${String(v)}`
        }
      }
      return null
    }
    if (typeof value === 'string' && !comps.includes(value)) {
      return `Geçersiz kompozisyon: ${value}`
    }
  }
  if (f === 'reservation.reservedfor.dayofweek' && op === 'in' && Array.isArray(value)) {
    const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday']
    for (const v of value) {
      if (typeof v !== 'string' || !days.includes(v)) {
        return `Geçersiz gün: ${String(v)}`
      }
    }
  }
  if (f === 'reservation.reservedfor.hour' && typeof value === 'number') {
    if (value < 0 || value > 23) return 'Saat 0–23 arasında olmalıdır'
  }
  if (
    (f === 'femaleratio' || f === 'maleratio' || f === 'venue.currentoccupancy') &&
    typeof value === 'number'
  ) {
    if (value < 0 || value > 1) return 'Oran 0.0–1.0 arasında olmalıdır'
  }
  return null
}

/**
 * custom_condition conditionsJson metnini doğrular (backend şeması ile uyumlu).
 */
export function validateCustomConditionJson(jsonText: string): CustomConditionValidationResult {
  const issues: CustomConditionValidationIssue[] = []
  const trimmed = jsonText.trim()

  if (!trimmed) {
    return {
      valid: false,
      issues: [{ path: '$', message: 'Koşul JSON boş olamaz', line: 1 }],
    }
  }

  let parsed: Record<string, unknown>
  try {
    parsed = JSON.parse(trimmed) as Record<string, unknown>
  } catch (e) {
    const msg = e instanceof Error ? e.message : 'Geçersiz JSON'
    return {
      valid: false,
      issues: [{ path: '$', message: msg, line: 1 }],
    }
  }

  if (parsed.version !== 1) {
    issues.push({
      path: 'version',
      message: 'version alanı zorunludur ve 1 olmalıdır',
      line: findLineForToken(trimmed, 'version'),
    })
  }

  const op = parsed.operator
  if (op !== 'and' && op !== 'or') {
    issues.push({
      path: 'operator',
      message: 'operator "and" veya "or" olmalıdır',
      line: findLineForToken(trimmed, 'operator'),
    })
  }

  const conditions = parsed.conditions
  if (!Array.isArray(conditions)) {
    issues.push({
      path: 'conditions',
      message: 'conditions bir dizi olmalıdır',
      line: findLineForToken(trimmed, 'conditions'),
    })
  } else if (conditions.length < 1) {
    issues.push({
      path: 'conditions',
      message: 'En az bir koşul tanımlanmalıdır',
      line: findLineForToken(trimmed, 'conditions'),
    })
  } else {
    conditions.forEach((item, index) => {
      const path = `conditions[${index}]`
      if (!item || typeof item !== 'object') {
        issues.push({ path, message: 'Koşul nesnesi geçersiz' })
        return
      }
      const c = item as Record<string, unknown>
      const field = typeof c.field === 'string' ? c.field : ''
      const fieldOp = typeof c.op === 'string' ? c.op : ''

      if (!field || !CUSTOM_CONDITION_FIELD_SET.has(field.toLowerCase())) {
        issues.push({
          path: `${path}.field`,
          message: field ? `Bilinmeyen alan: ${field}` : 'field zorunludur',
          line: field ? findLineForToken(trimmed, `"${field}"`) : undefined,
        })
      }
      if (!fieldOp || !CUSTOM_CONDITION_OPERATOR_SET.has(fieldOp.toLowerCase())) {
        issues.push({
          path: `${path}.op`,
          message: fieldOp ? `Bilinmeyen operatör: ${fieldOp}` : 'op zorunludur',
          line: fieldOp ? findLineForToken(trimmed, `"${fieldOp}"`) : undefined,
        })
      }
      const valueErr = isValidConditionValue(field, fieldOp, c.value)
      if (valueErr) {
        issues.push({
          path: `${path}.value`,
          message: valueErr,
          line: findLineForToken(trimmed, '"value"'),
        })
      }
    })
  }

  return { valid: issues.length === 0, issues }
}
