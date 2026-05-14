// API Response Types

export interface DashboardStats {
  todayReservations: number
  todayReservationsChange: number
  weekOccupancyRate: number
  weekOccupancyRateChange: number
  monthReservations: number
  monthReservationsLimit: number | null
  activeRulesCount: number
}

export interface WeeklyReservationData {
  date: string
  reservationCount: number
  occupancyRate: number
}

export interface TodayReservation {
  id: string
  guestName: string
  guestPhone: string
  partySize: number
  reservedFor: string
  tableName: string
  status: 'Pending' | 'Confirmed' | 'Seated' | 'Completed' | 'Cancelled' | 'NoShow'
}

export interface AuditLogEntry {
  id: string
  userId: string
  userName: string
  action: string
  entityType: string
  entityId: string
  details: string
  timestamp: string
}

export interface RuleStats {
  ruleId: string
  ruleName: string
  timesTriggered: number
  lastTriggeredAt: string | null
}

export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
}

export type TableLocation = 'Indoor' | 'Outdoor' | 'Balcony' | 'Bar' | 'Private' | 'Terrace' | 'Garden'

export interface Table {
  id: string
  venueId: string
  name: string
  capacity: number
  location: TableLocation
  description?: string
  isActive: boolean
  displayOrder: number
  createdAt: string
  updatedAt: string
}

export interface TableCombination {
  id: string
  venueId: string
  name: string
  tableIds: string[]
  /** Populated by some clients; API list returns only {@link tableIds}. */
  tables?: Table[]
  combinedCapacity: number
  isActive: boolean
  createdAt: string
}

export interface Venue {
  id: string
  tenantId: string
  name: string
  slug: string
  isActive: boolean
  tableCount: number
}

/** POST /api/v1/venue — CreateVenueDto ile uyumlu gövde. */
export interface CreateVenuePayload {
  name: string
  address?: string | null
  phoneNumber?: string | null
  description?: string | null
  timeZone?: string
  slotDurationMinutes?: number
  depositEnabled?: boolean
  depositPerPerson?: boolean
  /** DepositRefundPolicy: NoRefund = 2 (varsayılan). */
  depositRefundPolicy?: number
  depositRefundHours?: number | null
  depositPartialPercent?: number | null
  workingHours?: string | null
}

export interface PlanLimits {
  maxTables: number | null
  currentTableCount: number
  maxRules: number | null
  currentRuleCount: number
}

// Rule Types
export type RuleType = 
  | 'EarlyBooking'
  | 'VipPriority'
  | 'LargeGroup'
  | 'DepositRequired'
  | 'PeakHour'
  | 'TableCooldown'
  | 'GroupComposition'
  | 'CustomCondition'

export type RuleTrigger = 'OnReservation' | 'OnSeating' | 'OnModification' | 'OnCancellation'

export type ActionType = 'Block' | 'Warn' | 'Suggest' | 'Discount' | 'Deposit' | 'Redirect'

export type ConditionOperator = 
  | 'Equals' 
  | 'NotEquals' 
  | 'GreaterThan' 
  | 'GreaterThanOrEquals' 
  | 'LessThan' 
  | 'LessThanOrEquals'
  | 'Contains'
  | 'In'
  | 'Between'

export type LogicalOperator = 'And' | 'Or'

export interface RuleCondition {
  field: string
  operator: ConditionOperator
  value: string | number | boolean | string[] | number[]
  logicalOperator?: LogicalOperator
}

export interface RuleConditionGroup {
  conditions: RuleCondition[]
  logicalOperator: LogicalOperator
}

export interface RuleAction {
  type: ActionType
  message?: string
  discountPercent?: number
  depositAmount?: number
  depositPerPerson?: boolean
  useVenueDefault?: boolean
  redirectUrl?: string
  suggestedTableIds?: string[]
}

export interface Rule {
  id: string
  venueId: string
  name: string
  description?: string
  ruleType: RuleType
  trigger: RuleTrigger
  priority: number
  isActive: boolean
  conditions: RuleConditionGroup
  actions: RuleAction[]
  timesTriggered: number
  lastTriggeredAt?: string
  createdAt: string
  updatedAt: string
}

export interface RuleTemplate {
  id: string
  name: string
  description: string
  ruleType: RuleType
  icon: string
  defaultConditions: Partial<RuleConditionGroup>
  defaultActions: Partial<RuleAction>[]
  parameters: RuleTemplateParameter[]
}

export interface RuleTemplateParameter {
  name: string
  label: string
  type: 'number' | 'text' | 'select' | 'boolean' | 'time' | 'days'
  defaultValue: any
  options?: { value: string; label: string }[]
  min?: number
  max?: number
  required?: boolean
}

export interface RuleTestContext {
  partySize: number
  daysInAdvance: number
  customerTier: 'Regular' | 'Vip' | 'Blacklisted'
  dayOfWeek: number
  hour: number
  occupancyPercent: number
  hasChildren?: boolean
  childCount?: number
}

export interface RuleTestResult {
  triggered: boolean
  outcome?: {
    action: ActionType
    message?: string
    payload?: Record<string, any>
  }
  executionTimeMs: number
  evaluationPath?: string[]
}

// Reservation Types
export type ReservationStatus = 
  | 'Pending' 
  | 'Confirmed' 
  | 'Seated' 
  | 'Completed' 
  | 'Cancelled' 
  | 'NoShow'

export interface Reservation {
  id: string
  venueId: string
  venueName: string
  tableId?: string | null
  tableName?: string | null
  tableCombinationId?: string | null
  tableCombinationName?: string | null
  customerId?: string | null
  guestName: string
  guestEmail?: string | null
  guestPhone: string
  customerTier?: string | null
  partySize: number
  reservedFor: string
  endTime: string
  status: ReservationStatus
  source: string
  confirmCode: string
  specialRequests?: string | null
  internalNotes?: string | null
  discountPercent?: number | null
  depositStatus: string
  depositAmount?: number | null
  depositPaidAt?: string | null
  cancellationReason?: string | null
  cancelledAt?: string | null
  createdAt: string
  updatedAt?: string
  customFieldAnswers?: Record<string, string> | null
  modifiedFromReservationId?: string | null
  /** Parsed snapshot when API exposes it as JSON array (optional). */
  appliedRules?: string[]
}

export interface ReservationStatusLog {
  id: string
  reservationId: string
  fromStatus: ReservationStatus | null
  toStatus: ReservationStatus
  userId: string
  userName: string
  reason?: string
  timestamp: string
}

export type CustomerTier = 'Regular' | 'Gold' | 'VIP' | 'Blacklisted'

export interface Customer {
  id: string
  tenantId: string
  fullName: string
  email?: string
  phone?: string
  tier: CustomerTier
  totalVisits?: number
  lastReservationDate?: string
  isBlacklisted: boolean
  blacklistReason?: string
  notes?: string
  createdAt: string
  updatedAt: string
}

export interface TimeSlot {
  time: string
  tableId: string
  tableName: string
  isAvailable: boolean
  existingReservation?: Reservation
}

export interface VenueCustomField {
  id: string
  name: string
  label: string
  type: 'text' | 'number' | 'boolean' | 'select' | 'textarea'
  required: boolean
  options?: string[]
  placeholder?: string
}

export interface ReservationEvaluationResult {
  isAllowed: boolean
  warnings?: { message: string; ruleId: string }[]
  blockers?: { message: string; ruleId: string }[]
  suggestedTableIds?: string[]
  discountPercent?: number
  depositRequired?: boolean
  depositAmount?: number
  appliedRules?: string[]
}

// Staff Types
export interface StaffMember {
  id: string
  tenantId: string
  fullName: string
  email: string
  role: 'Owner' | 'Staff'
  isActive: boolean
  lastLoginAt?: string
  createdAt: string
  updatedAt: string
}

export interface StaffInvite {
  id: string
  tenantId: string
  email: string
  role: 'Owner' | 'Staff'
  token: string
  expiresAt: string
  createdAt: string
}
