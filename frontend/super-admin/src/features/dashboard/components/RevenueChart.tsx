import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

interface RevenueChartProps {
  mrr: number
}

export function RevenueChart({ mrr }: RevenueChartProps) {
  // Geçmiş veri yokken mevcut MRR'yi son noktaya koyuyoruz
  const data = [
    { ay: 'Oca', mrr: 0 },
    { ay: 'Şub', mrr: 0 },
    { ay: 'Mar', mrr: 0 },
    { ay: 'Nis', mrr: 0 },
    { ay: 'May', mrr: 0 },
    { ay: 'Haz', mrr },
  ]

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Aylık Gelir Trendi (MRR)</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={220}>
          <AreaChart data={data} margin={{ top: 4, right: 4, left: 0, bottom: 0 }}>
            <defs>
              <linearGradient id="mrrGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#6366f1" stopOpacity={0.3} />
                <stop offset="95%" stopColor="#6366f1" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
            <XAxis dataKey="ay" tick={{ fontSize: 12, fill: 'hsl(var(--muted-foreground))' }} />
            <YAxis
              tick={{ fontSize: 12, fill: 'hsl(var(--muted-foreground))' }}
              tickFormatter={(v: number) => `₺${(v / 1000).toFixed(0)}K`}
            />
            <Tooltip
              formatter={(value: number) => [`₺${value.toLocaleString('tr-TR')}`, 'MRR']}
              contentStyle={{ backgroundColor: 'hsl(var(--card))', border: '1px solid hsl(var(--border))', borderRadius: '8px' }}
            />
            <Area
              type="monotone"
              dataKey="mrr"
              stroke="#6366f1"
              strokeWidth={2}
              fill="url(#mrrGradient)"
            />
          </AreaChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}
