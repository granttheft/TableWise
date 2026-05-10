import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Plus } from 'lucide-react'

export function TablesPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Masalar</h1>
          <p className="text-muted-foreground">Masa düzeninizi yönetin</p>
        </div>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          Yeni Masa
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Masa Listesi</CardTitle>
          <CardDescription>Tüm masalarınızı buradan yönetebilirsiniz</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Masa listesi gelecek...</p>
        </CardContent>
      </Card>
    </div>
  )
}
