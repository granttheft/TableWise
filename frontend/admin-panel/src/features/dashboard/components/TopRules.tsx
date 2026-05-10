import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Badge } from '@/components/ui/badge'
import { Zap } from 'lucide-react'
import { formatDistanceToNow, parseISO } from 'date-fns'
import { tr } from 'date-fns/locale'
import type { RuleStats } from '@/types/api'

interface TopRulesProps {
  data?: RuleStats[]
  isLoading?: boolean
}

export function TopRules({ data, isLoading }: TopRulesProps) {
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>
        <CardContent className="space-y-4">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="flex items-center justify-between">
              <Skeleton className="h-4 w-48" />
              <Skeleton className="h-6 w-12" />
            </div>
          ))}
        </CardContent>
      </Card>
    )
  }

  if (!data || data.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>En Sık Tetiklenen Kurallar</CardTitle>
          <CardDescription>Top 5 kural</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="py-8 text-center text-sm text-muted-foreground">
            Henüz tetiklenen kural yok
          </div>
        </CardContent>
      </Card>
    )
  }

  const topFive = data.slice(0, 5)

  return (
    <Card>
      <CardHeader>
        <CardTitle>En Sık Tetiklenen Kurallar</CardTitle>
        <CardDescription>Top 5 kural</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {topFive.map((rule, index) => (
          <div key={rule.ruleId} className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <div className="flex h-6 w-6 items-center justify-center rounded-full bg-accent text-xs font-medium text-accent-foreground">
                {index + 1}
              </div>
              <div>
                <p className="text-sm font-medium">{rule.ruleName}</p>
                {rule.lastTriggeredAt && (
                  <p className="text-xs text-muted-foreground">
                    Son: {formatDistanceToNow(parseISO(rule.lastTriggeredAt), { addSuffix: true, locale: tr })}
                  </p>
                )}
              </div>
            </div>
            <Badge variant="secondary" className="flex items-center space-x-1">
              <Zap className="h-3 w-3" />
              <span>{rule.timesTriggered}</span>
            </Badge>
          </div>
        ))}
      </CardContent>
    </Card>
  )
}
