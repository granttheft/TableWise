import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Search } from 'lucide-react'

export function CustomersPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Müşteriler</h1>
          <p className="text-muted-foreground">Müşteri veritabanınızı yönetin</p>
        </div>
        <div className="relative w-64">
          <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input placeholder="Müşteri ara..." className="pl-8" />
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Müşteri Listesi</CardTitle>
          <CardDescription>Tüm müşterileriniz ve rezervasyon geçmişleri</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Müşteri listesi gelecek...</p>
        </CardContent>
      </Card>
    </div>
  )
}
