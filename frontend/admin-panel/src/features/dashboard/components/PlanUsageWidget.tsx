import { Building2, Table2, BookOpen, CalendarDays, Sparkles } from 'lucide-react'
import { usePlanLimits, isAtLimit, isNearLimit, limitPercentage } from '@/hooks/usePlanLimits'
import { Skeleton } from '@/components/ui/skeleton'

interface LimitRowProps {
  icon: React.ReactNode
  label: string
  current: number
  max: number | null
}

function LimitRow({ icon, label, current, max }: LimitRowProps) {
  const atLimit   = isAtLimit(current, max)
  const nearLimit = isNearLimit(current, max)
  const pct       = limitPercentage(current, max)
  const unlimited = max === null

  return (
    <div className="space-y-1.5">
      <div className="flex items-center justify-between text-sm">
        <span className="flex items-center gap-1.5 text-muted-foreground">
          {icon}
          {label}
        </span>
        <span className={
          atLimit   ? 'text-destructive font-medium' :
          nearLimit ? 'text-amber-500 font-medium'   :
                      'text-foreground'
        }>
          {unlimited ? `${current} / ∞` : `${current} / ${max}`}
        </span>
      </div>
      <div className="h-1.5 w-full rounded-full bg-muted overflow-hidden">
        {!unlimited && (
          <div
            className={`h-full rounded-full transition-all ${
              atLimit   ? 'bg-destructive' :
              nearLimit ? 'bg-amber-500'   :
                          'bg-primary'
            }`}
            style={{ width: `${pct}%` }}
          />
        )}
      </div>
    </div>
  )
}

export function PlanUsageWidget() {
  const { data: limits, isLoading } = usePlanLimits()

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(4)].map((_, i) => <Skeleton key={i} className="h-8" />)}
      </div>
    )
  }

  if (!limits) return null

  return (
    <div className="space-y-3">
      {limits.hasCustomLimits && (
        <div className="flex items-center gap-1.5 text-xs text-amber-500 pb-1">
          <Sparkles className="h-3 w-3" />
          Özel limitler uygulanıyor
        </div>
      )}
      <LimitRow
        icon={<Building2 className="h-3.5 w-3.5" />}
        label="Mekan"
        current={limits.currentVenueCount}
        max={limits.maxVenues}
      />
      <LimitRow
        icon={<Table2 className="h-3.5 w-3.5" />}
        label="Masa"
        current={limits.currentTableCount}
        max={limits.maxTables}
      />
      <LimitRow
        icon={<BookOpen className="h-3.5 w-3.5" />}
        label="Kural"
        current={limits.currentRuleCount}
        max={limits.maxRules}
      />
      <LimitRow
        icon={<CalendarDays className="h-3.5 w-3.5" />}
        label="Aylık Rezervasyon"
        current={limits.currentReservationCount}
        max={limits.maxReservationsPerMonth}
      />
    </div>
  )
}
