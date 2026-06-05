import { Building2, TrendingUp, UserCheck, Clock, UserX, BadgeDollarSign } from 'lucide-react'
import { usePlatformStats } from '@/hooks/usePlatformStats'
import { StatCard } from './components/StatCard'
import { RevenueChart } from './components/RevenueChart'
import { PlanDistChart } from './components/PlanDistChart'

export function DashboardPage() {
  const { data: stats, isLoading } = usePlatformStats()

  const formatMrr = (mrr: number) =>
    new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(mrr)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <p className="text-sm text-muted-foreground">Platform geneli istatistikler</p>
      </div>

      {/* Stat Cards */}
      <div className="grid grid-cols-2 gap-4 md:grid-cols-3 xl:grid-cols-6">
        <StatCard
          label="Toplam Müşteri"
          value={stats?.totalTenants ?? '—'}
          icon={Building2}
          loading={isLoading}
        />
        <StatCard
          label="Aktif"
          value={stats?.activeTenants ?? '—'}
          icon={UserCheck}
          loading={isLoading}
        />
        <StatCard
          label="Trial"
          value={stats?.trialTenants ?? '—'}
          icon={Clock}
          loading={isLoading}
        />
        <StatCard
          label="Askıya Alınan"
          value={stats?.suspendedTenants ?? '—'}
          icon={UserX}
          loading={isLoading}
        />
        <StatCard
          label="Bu Ay Yeni"
          value={stats?.newTenantsThisMonth ?? '—'}
          icon={TrendingUp}
          loading={isLoading}
        />
        <StatCard
          label="MRR"
          value={stats ? formatMrr(stats.mrr) : '—'}
          icon={BadgeDollarSign}
          loading={isLoading}
          highlight
        />
      </div>

      {/* Charts */}
      {stats && (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
          <div className="lg:col-span-2">
            <RevenueChart mrr={stats.mrr} />
          </div>
          <div>
            <PlanDistChart stats={stats} />
          </div>
        </div>
      )}
    </div>
  )
}
