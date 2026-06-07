export type PlatformRole = 'SuperAdmin' | 'Marketing' | 'Finance'

export interface PlatformUser {
  email: string
  fullName: string
  role: PlatformRole
}

export interface PlatformAuthResult {
  accessToken: string
  fullName: string
  role: PlatformRole
}

export interface PlatformStatsDto {
  totalTenants: number
  activeTenants: number
  trialTenants: number
  suspendedTenants: number
  newTenantsThisMonth: number
  mrr: number
}

export type PlanStatus = 'Active' | 'TrialActive' | 'TrialExpired' | 'Suspended' | 'Cancelled'

export interface TenantSummaryDto {
  id: string
  name: string
  email: string
  planName: string
  planStatus: PlanStatus
  createdAt: string
  reservationCountThisMonth: number
  isActive: boolean
}

export interface PlatformNoteDto {
  id: string
  content: string
  createdByEmail: string
  createdAt: string
}

export interface TenantDetailDto extends TenantSummaryDto {
  slug: string
  trialEndsAt?: string
  planRenewsAt?: string
  venueCount: number
  userCount: number
  notes: PlatformNoteDto[]
  subscriptionAmount?: number
  subscriptionPeriodStart?: string
  subscriptionPeriodEnd?: string
}

export interface PlatformUserDto {
  id: string
  email: string
  fullName: string
  role: PlatformRole
  isActive: boolean
  lastLoginAt: string | null
  createdAt: string
}

export interface InvitePlatformUserDto {
  email: string
  fullName: string
  role: PlatformRole
  password: string
}

export interface UpdatePlatformUserRoleDto {
  newRole: PlatformRole
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}
