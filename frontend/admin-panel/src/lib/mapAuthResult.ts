import type { User } from '@/stores/authStore'

/** Backend AuthResultDto JSON (camelCase). */
export interface AuthResultPayload {
  tokens: {
    accessToken: string
    refreshToken: string
    accessTokenExpiresAt?: string
    refreshTokenExpiresAt?: string
  }
  user: {
    id: string
    email: string
    firstName: string
    lastName: string
    role: number | string
    isEmailVerified: boolean
  }
  tenant: {
    id: string
    name: string
    slug: string
    planTier: number | string
    planStatus: number | string
    trialEndsAt?: string | null
  }
}

const planTierLabels = ['Starter', 'Pro', 'Business', 'Enterprise'] as const

const userRoleLabels = ['SuperAdmin', 'Owner', 'Staff'] as const

/**
 * Maps numeric or string enum from API to display plan name.
 */
function mapPlanTier(tier: number | string): string {
  if (typeof tier === 'string') return tier
  return planTierLabels[tier] ?? 'Starter'
}

/**
 * Maps API plan status to store union used by the admin shell.
 */
function mapPlanStatus(status: number | string): User['planStatus'] {
  if (typeof status === 'string') {
    const s = status as User['planStatus']
    if (
      s === 'Active' ||
      s === 'TrialActive' ||
      s === 'TrialExpired' ||
      s === 'Suspended' ||
      s === 'Cancelled'
    ) {
      return s
    }
  }
  switch (status) {
    case 0:
      return 'TrialActive'
    case 1:
      return 'Active'
    case 2:
      return 'TrialExpired'
    case 3:
      return 'Suspended'
    case 4:
      return 'Cancelled'
    default:
      return 'Active'
  }
}

/**
 * Maps API user role to string for the auth store.
 */
function mapUserRole(role: number | string): string {
  if (typeof role === 'string') return role
  return userRoleLabels[role] ?? 'Staff'
}

/**
 * Converts AuthResultDto body to the shape expected by useAuthStore.login.
 */
export function mapAuthResultToUser(payload: AuthResultPayload): User {
  const { user: u, tenant: t } = payload
  const name = [u.firstName, u.lastName].filter(Boolean).join(' ').trim() || u.email
  return {
    id: u.id,
    email: u.email,
    name,
    role: mapUserRole(u.role),
    tenantId: t.id,
    tenantName: t.name,
    planName: mapPlanTier(t.planTier),
    planStatus: mapPlanStatus(t.planStatus),
    trialEndsAt: t.trialEndsAt ?? undefined,
  }
}

/**
 * Reads user-visible message from ASP.NET ProblemDetails or generic API errors.
 */
export function getApiErrorMessage(error: unknown, fallback: string): string {
  const err = error as {
    response?: { data?: { detail?: string; title?: string; message?: string } }
  }
  const data = err.response?.data
  if (data && typeof data.detail === 'string' && data.detail.length > 0) {
    return data.detail
  }
  if (data && typeof data.message === 'string' && data.message.length > 0) {
    return data.message
  }
  if (data && typeof data.title === 'string' && data.title.length > 0) {
    return data.title
  }
  return fallback
}
