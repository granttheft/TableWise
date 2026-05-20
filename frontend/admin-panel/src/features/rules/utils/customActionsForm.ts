import { DEFAULT_CUSTOM_ACTIONS_JSON } from '../constants/customConditionExamples'

/** Özel kural aksiyon formu (UI); JSON ile senkron. */
export interface CustomActionsFormState {
  block: boolean
  warn: boolean
  suggest: boolean
  requireDeposit: boolean
  message: string
  discountPercent: number | null
  depositAmount: number | null
}

export const DEFAULT_CUSTOM_ACTIONS_FORM: CustomActionsFormState = {
  block: false,
  warn: true,
  suggest: false,
  requireDeposit: false,
  message: '',
  discountPercent: null,
  depositAmount: 100,
}

/**
 * actionsJson metnini forma çevirir; parse hatasında varsayılan döner.
 */
export function parseCustomActionsJson(jsonText: string): CustomActionsFormState {
  try {
    const a = JSON.parse(jsonText) as Record<string, unknown>
    return {
      block: a.block === true,
      warn: a.warn === true,
      suggest: a.suggest === true,
      requireDeposit: a.requireDeposit === true,
      message: typeof a.message === 'string' ? a.message : '',
      discountPercent:
        typeof a.discountPercent === 'number' && a.discountPercent > 0
          ? Math.min(50, Math.max(0, a.discountPercent))
          : null,
      depositAmount:
        typeof a.depositAmount === 'number' && a.depositAmount >= 0
          ? a.depositAmount
          : 100,
    }
  } catch {
    return { ...DEFAULT_CUSTOM_ACTIONS_FORM }
  }
}

/**
 * Form durumundan API actionsJson üretir.
 */
export function buildCustomActionsJsonFromForm(form: CustomActionsFormState): string {
  const discount =
    form.discountPercent != null && form.discountPercent > 0
      ? Math.min(50, form.discountPercent)
      : null

  return JSON.stringify({
    version: 1,
    block: form.block,
    warn: form.warn,
    suggest: form.suggest,
    requireDeposit: form.requireDeposit,
    discountPercent: discount,
    depositAmount: form.requireDeposit ? form.depositAmount ?? 100 : null,
    message: form.message ?? '',
  })
}

/**
 * Boş/varsayılan aksiyon JSON.
 */
export function getDefaultCustomActionsJson(): string {
  return DEFAULT_CUSTOM_ACTIONS_JSON
}
