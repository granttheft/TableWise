import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { UserPlus } from 'lucide-react'

export function StaffPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Ekip</h1>
          <p className="text-muted-foreground">Personel yetkilendirme ve yönetimi</p>
        </div>
        <Button>
          <UserPlus className="mr-2 h-4 w-4" />
          Personel Davet Et
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Ekip Üyeleri</CardTitle>
          <CardDescription>Admin ve personel listesi</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Personel listesi gelecek...</p>
        </CardContent>
      </Card>
    </div>
  )
}
