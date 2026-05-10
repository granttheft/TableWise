import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

export function SettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Ayarlar</h1>
        <p className="text-muted-foreground">Mekan ve hesap ayarlarınızı yapılandırın</p>
      </div>

      <div className="grid gap-4">
        <Card>
          <CardHeader>
            <CardTitle>Mekan Bilgileri</CardTitle>
            <CardDescription>Adres, çalışma saatleri ve iletişim bilgileri</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">Mekan ayarları gelecek...</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Hesap Ayarları</CardTitle>
            <CardDescription>Email, şifre ve bildirim tercihleri</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">Hesap ayarları gelecek...</p>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
