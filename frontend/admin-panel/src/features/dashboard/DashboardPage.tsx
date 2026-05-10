import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Calendar, Users, TableProperties, TrendingUp } from 'lucide-react'

const stats = [
  {
    title: 'Toplam Rezervasyon',
    value: '156',
    change: '+12%',
    icon: Calendar,
  },
  {
    title: 'Bugünkü Rezervasyonlar',
    value: '24',
    change: '+5%',
    icon: Calendar,
  },
  {
    title: 'Aktif Müşteriler',
    value: '342',
    change: '+18%',
    icon: Users,
  },
  {
    title: 'Doluluk Oranı',
    value: '87%',
    change: '+3%',
    icon: TrendingUp,
  },
]

export function DashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Mekanınızın genel durumunu görüntüleyin</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <Card key={stat.title}>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{stat.title}</CardTitle>
              <stat.icon className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
              <p className="text-xs text-muted-foreground">
                <span className="text-green-500">{stat.change}</span> geçen aya göre
              </p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Son Rezervasyonlar</CardTitle>
            <CardDescription>En son eklenen rezervasyonlar</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">Rezervasyon listesi gelecek...</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Masa Durumu</CardTitle>
            <CardDescription>Masaların anlık durumu</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">Masa durumları gelecek...</p>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
