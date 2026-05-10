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
