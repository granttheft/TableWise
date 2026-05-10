import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { TrendingUp, TrendingDown, LucideIcon } from 'lucide-react'
import { cn } from '@/lib/cn'

interface StatCardProps {
  title: string
  value: string | number
  change?: number
  icon: LucideIcon
  isLoading?: boolean
  showProgress?: boolean
  progress?: number
}

export function StatCard({
  title,
  value,
  change,
  icon: Icon,
  isLoading,
  showProgress,
  progress,
}: StatCardProps) {
  if (isLoading) {
    return (
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-4 w-4 rounded" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-8 w-20 mb-2" />
          <Skeleton className="h-3 w-32" />
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        <Icon className="h-4 w-4 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold">{value}</div>
        {change !== undefined && (
          <p className="flex items-center text-xs text-muted-foreground">
            {change > 0 ? (
              <TrendingUp className="mr-1 h-3 w-3 text-green-500" />
            ) : change < 0 ? (
              <TrendingDown className="mr-1 h-3 w-3 text-red-500" />
            ) : null}
            <span className={cn(change > 0 ? 'text-green-500' : change < 0 ? 'text-red-500' : '')}>
              {change > 0 ? '+' : ''}
              {change}
            </span>
            <span className="ml-1">önceki döneme göre</span>
          </p>
        )}
        {showProgress && progress !== undefined && (
          <div className="mt-2">
            <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
              <div
                className="h-full bg-accent transition-all"
                style={{ width: `${Math.min(progress, 100)}%` }}
              />
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
