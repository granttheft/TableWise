import { Calendar, Percent, FileCheck, Zap } from 'lucide-react'
import { useDashboardStats } from '@/hooks/useDashboardStats'
import { useTodayReservations } from '@/hooks/useTodayReservations'
import { useWeeklyData } from '@/hooks/useWeeklyData'
import { useAuditLogRecent } from '@/hooks/useAuditLogRecent'
import { useRuleStats } from '@/hooks/useRuleStats'
import { StatCard } from './components/StatCard'
import { WeeklyChart } from './components/WeeklyChart'
import { TodayReservationsList } from './components/TodayReservationsList'
import { RecentActivity } from './components/RecentActivity'
import { TopRules } from './components/TopRules'

export function DashboardPage() {
  const { data: stats, isLoading: statsLoading } = useDashboardStats()
  const { data: todayReservations, isLoading: todayLoading } = useTodayReservations()
  const { data: weeklyData, isLoading: weeklyLoading } = useWeeklyData()
  const { data: auditLogs, isLoading: auditLoading } = useAuditLogRecent()
  const { data: ruleStats, isLoading: rulesLoading } = useRuleStats()

  const monthProgress = stats?.monthReservationsLimit
    ? (stats.monthReservations / stats.monthReservationsLimit) * 100
    : undefined

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Mekanınızın genel durumunu görüntüleyin</p>
      </div>

      {/* Stat Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Bugünkü Rezervasyonlar"
          value={stats?.todayReservations || 0}
          change={stats?.todayReservationsChange}
          icon={Calendar}
          isLoading={statsLoading}
        />
        <StatCard
          title="Bu Hafta Doluluk"
          value={stats ? `${stats.weekOccupancyRate}%` : '0%'}
          change={stats?.weekOccupancyRateChange}
          icon={Percent}
          isLoading={statsLoading}
        />
        <StatCard
          title="Bu Ay Rezervasyon"
          value={
            stats
              ? stats.monthReservationsLimit
                ? `${stats.monthReservations} / Plan Limiti: ${stats.monthReservationsLimit}`
                : `${stats.monthReservations} rezervasyon`
              : '0'
          }
          icon={FileCheck}
          isLoading={statsLoading}
          showProgress={!!stats?.monthReservationsLimit}
          progress={monthProgress}
        />
        <StatCard
          title="Aktif Kurallar"
          value={stats?.activeRulesCount || 0}
          icon={Zap}
          isLoading={statsLoading}
        />
      </div>

      {/* Weekly Chart + Today's Reservations */}
      <div className="grid gap-4 md:grid-cols-3">
        <WeeklyChart data={weeklyData} isLoading={weeklyLoading} />
        <TodayReservationsList data={todayReservations} isLoading={todayLoading} />
      </div>

      {/* Recent Activity + Top Rules */}
      <div className="grid gap-4 md:grid-cols-2">
        <RecentActivity data={auditLogs} isLoading={auditLoading} />
        <TopRules data={ruleStats} isLoading={rulesLoading} />
      </div>
    </div>
  )
}
