import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import { format, parseISO } from 'date-fns'
import { tr } from 'date-fns/locale'
import type { WeeklyReservationData } from '@/types/api'

interface WeeklyChartProps {
  data?: WeeklyReservationData[]
  isLoading?: boolean
}

export function WeeklyChart({ data, isLoading }: WeeklyChartProps) {
  if (isLoading) {
    return (
      <Card className="col-span-2">
        <CardHeader>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-[300px] w-full" />
        </CardContent>
      </Card>
    )
  }

  if (!data || data.length === 0) {
    return (
      <Card className="col-span-2">
        <CardHeader>
          <CardTitle>Son 7 Gün</CardTitle>
          <CardDescription>Rezervasyon ve doluluk oranı</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex h-[300px] items-center justify-center text-muted-foreground">
            Veri bulunamadı
          </div>
        </CardContent>
      </Card>
    )
  }

  const chartData = data.map((item) => ({
    date: format(parseISO(item.date), 'EEE', { locale: tr }),
    rezervasyon: item.reservationCount,
    doluluk: item.occupancyRate,
  }))

  return (
    <Card className="col-span-2">
      <CardHeader>
        <CardTitle>Son 7 Gün</CardTitle>
        <CardDescription>Rezervasyon sayısı ve doluluk oranı</CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
            <XAxis dataKey="date" className="text-xs" />
            <YAxis yAxisId="left" className="text-xs" />
            <YAxis yAxisId="right" orientation="right" className="text-xs" />
            <Tooltip
              contentStyle={{
                backgroundColor: 'hsl(var(--card))',
                border: '1px solid hsl(var(--border))',
                borderRadius: '6px',
              }}
            />
            <Legend />
            <Bar yAxisId="left" dataKey="rezervasyon" fill="hsl(var(--primary))" name="Rezervasyon" />
            <Bar yAxisId="right" dataKey="doluluk" fill="#f59e0b" name="Doluluk %" />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}
