import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { formatDistanceToNow, parseISO } from 'date-fns'
import { tr } from 'date-fns/locale'
import type { AuditLogEntry } from '@/types/api'

interface RecentActivityProps {
  data?: AuditLogEntry[]
  isLoading?: boolean
}

export function RecentActivity({ data, isLoading }: RecentActivityProps) {
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>
        <CardContent className="space-y-4">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="flex items-start space-x-4">
              <Skeleton className="h-8 w-8 rounded-full" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-3 w-24" />
              </div>
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
          <CardTitle>Son Aktiviteler</CardTitle>
          <CardDescription>Sistem aktivite kaydı</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="py-8 text-center text-sm text-muted-foreground">
            Henüz aktivite kaydı yok
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Son Aktiviteler</CardTitle>
        <CardDescription>Sistem aktivite kaydı</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {data.map((log) => (
          <div key={log.id} className="flex items-start space-x-4 text-sm">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-accent text-accent-foreground">
              {log.userName.charAt(0).toUpperCase()}
            </div>
            <div className="flex-1 space-y-1">
              <p className="leading-tight">
                <span className="font-medium">{log.userName}</span>{' '}
                <span className="text-muted-foreground">{log.action}</span>
              </p>
              <p className="text-xs text-muted-foreground">
                {formatDistanceToNow(parseISO(log.timestamp), { addSuffix: true, locale: tr })}
              </p>
            </div>
          </div>
        ))}
      </CardContent>
    </Card>
  )
}
