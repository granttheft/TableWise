import { useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Switch } from '@/components/ui/switch'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Mail, MessageSquare, Info } from 'lucide-react'
import { toast } from 'sonner'

interface NotificationSetting {
  id: string
  label: string
  description: string
  emailEnabled: boolean
  smsEnabled: boolean
}

export function NotificationSettings() {
  const [currentPlan] = useState('Starter') // TODO: Get from auth store/API
  const isProOrAbove = currentPlan === 'Pro' || currentPlan === 'Business' || currentPlan === 'Enterprise'

  const [settings, setSettings] = useState<NotificationSetting[]>([
    {
      id: 'new-reservation',
      label: 'Yeni Rezervasyon',
      description: 'Yeni rezervasyon oluşturulduğunda',
      emailEnabled: true,
      smsEnabled: false,
    },
    {
      id: 'reservation-cancelled',
      label: 'Rezervasyon İptali',
      description: 'Müşteri rezervasyonu iptal ettiğinde',
      emailEnabled: true,
      smsEnabled: false,
    },
    {
      id: 'reservation-modified',
      label: 'Rezervasyon Değişikliği',
      description: 'Rezervasyon detayları değiştirildiğinde',
      emailEnabled: true,
      smsEnabled: false,
    },
    {
      id: 'reservation-reminder',
      label: 'Rezervasyon Hatırlatıcı',
      description: 'Rezervasyondan 24 saat önce müşteriye',
      emailEnabled: true,
      smsEnabled: false,
    },
    {
      id: 'deposit-received',
      label: 'Kapora Alındı',
      description: 'Kapora ödemesi tamamlandığında',
      emailEnabled: true,
      smsEnabled: false,
    },
    {
      id: 'no-show',
      label: 'Gelmedi (No-Show)',
      description: 'Müşteri rezervasyona gelmediğinde',
      emailEnabled: false,
      smsEnabled: false,
    },
  ])

  const toggleEmail = (id: string) => {
    setSettings(
      settings.map((s) =>
        s.id === id ? { ...s, emailEnabled: !s.emailEnabled } : s
      )
    )
  }

  const toggleSMS = (id: string) => {
    if (!isProOrAbove) {
      toast.error('SMS bildirimleri Pro plandan itibaren kullanılabilir')
      return
    }
    setSettings(
      settings.map((s) =>
        s.id === id ? { ...s, smsEnabled: !s.smsEnabled } : s
      )
    )
  }

  const handleSave = () => {
    console.log('Notification settings:', settings)
    toast.success('Bildirim ayarları kaydedildi')
  }

  return (
    <div className="space-y-6">
      <Alert>
        <Info className="h-4 w-4" />
        <AlertDescription>
          Email bildirimleri tüm planlarda ücretsizdir. SMS bildirimleri Pro plandan itibaren kullanılabilir.
        </AlertDescription>
      </Alert>

      <Card>
        <CardHeader>
          <CardTitle>Bildirim Tercihleri</CardTitle>
          <CardDescription>Hangi durumlarda bildirim almak istediğinizi seçin</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {settings.map((setting) => (
            <div key={setting.id} className="flex items-start justify-between border-b pb-4 last:border-0">
              <div className="space-y-1">
                <Label className="text-base">{setting.label}</Label>
                <p className="text-sm text-muted-foreground">{setting.description}</p>
              </div>
              <div className="flex gap-4">
                <div className="flex items-center gap-2">
                  <Mail className="h-4 w-4 text-muted-foreground" />
                  <Switch
                    checked={setting.emailEnabled}
                    onCheckedChange={() => toggleEmail(setting.id)}
                  />
                </div>
                <div className="flex items-center gap-2">
                  <MessageSquare className="h-4 w-4 text-muted-foreground" />
                  <Switch
                    checked={setting.smsEnabled}
                    onCheckedChange={() => toggleSMS(setting.id)}
                    disabled={!isProOrAbove}
                  />
                  {!isProOrAbove && (
                    <Badge variant="secondary" className="ml-1">
                      Pro+
                    </Badge>
                  )}
                </div>
              </div>
            </div>
          ))}

          <Button onClick={handleSave} className="mt-4">
            Kaydet
          </Button>
        </CardContent>
      </Card>

      {!isProOrAbove && (
        <Alert>
          <MessageSquare className="h-4 w-4" />
          <AlertDescription>
            SMS bildirimleri Pro plandan itibaren kullanılabilir.{' '}
            <a href="/subscription" className="font-medium underline">
              Planınızı yükseltin
            </a>
          </AlertDescription>
        </Alert>
      )}
    </div>
  )
}
