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
  tables: Table[]
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

export interface PlanLimits {
  maxTables: number | null
  currentTableCount: number
}
