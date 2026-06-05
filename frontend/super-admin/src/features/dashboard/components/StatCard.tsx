import { type LucideIcon } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/cn'

interface StatCardProps {
  label: string
  value: string | number
  icon: LucideIcon
  loading?: boolean
  highlight?: boolean
}

export function StatCard({ label, value, icon: Icon, loading, highlight }: StatCardProps) {
  return (
    <Card className={cn(highlight && 'border-primary/40 bg-primary/5')}>
      <CardContent className="pt-6">
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">{label}</p>
            {loading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <p className="text-2xl font-bold">{value}</p>
            )}
          </div>
          <div className={cn('rounded-lg p-2', highlight ? 'bg-primary/20' : 'bg-muted')}>
            <Icon className={cn('h-5 w-5', highlight ? 'text-primary' : 'text-muted-foreground')} />
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
