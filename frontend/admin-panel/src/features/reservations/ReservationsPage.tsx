import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Plus } from 'lucide-react'

export function ReservationsPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Rezervasyonlar</h1>
          <p className="text-muted-foreground">Tüm rezervasyonlarınızı görüntüleyin</p>
        </div>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          Manuel Rezervasyon
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Rezervasyon Listesi</CardTitle>
          <CardDescription>Günlük rezervasyon takibi</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Rezervasyon listesi gelecek...</p>
        </CardContent>
      </Card>
    </div>
  )
}
