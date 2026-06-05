import { useState } from 'react'
import { MessageCircle, Send, Wifi, WifiOff, AlertCircle, Info } from 'lucide-react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Switch } from '@/components/ui/switch'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Input } from '@/components/ui/input'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import {
  useWhatsAppSettings,
  useUpdateWhatsAppSettings,
  useSendTestMessage,
} from '@/hooks/useWhatsAppSettings'

interface WhatsAppSettingsProps {
  venueId: string | undefined
}

interface NotificationToggle {
  key: 'notifyReservationReceived' | 'notifyReservationConfirmed' | 'notifyReminder' | 'notifyCancellation'
  label: string
  description: string
  comingSoon?: boolean
}

const NOTIFICATION_TOGGLES: NotificationToggle[] = [
  {
    key: 'notifyReservationReceived',
    label: 'Rezervasyon Alındı',
    description: 'Müşteri rezervasyon yaptığında otomatik bildirim gönderilir.',
  },
  {
    key: 'notifyReservationConfirmed',
    label: 'Rezervasyon Onaylandı',
    description: 'Rezervasyon onaylandığında müşteriye bildirim gönderilir.',
  },
  {
    key: 'notifyReminder',
    label: 'Hatırlatma',
    description: 'Rezervasyondan 1 gün önce otomatik hatırlatma mesajı gönderilir.',
  },
  {
    key: 'notifyCancellation',
    label: 'İptal Bildirimi',
    description: 'Rezervasyon iptal edildiğinde müşteriye bildirim gönderilir.',
  },
]

const COMING_SOON_TOGGLES = [
  { label: 'Ödeme Başarısız', description: 'Kapora ödemesi başarısız olduğunda bildirim. (Faz 7.5)' },
  { label: 'İade Bildirimi', description: 'Kapora iadesi gerçekleştiğinde bildirim. (Faz 7.5)' },
]

export function WhatsAppSettings({ venueId }: WhatsAppSettingsProps) {
  const [testPhone, setTestPhone] = useState('')
  const { data: settings, isLoading } = useWhatsAppSettings(venueId)
  const updateMutation = useUpdateWhatsAppSettings(venueId)
  const testMutation = useSendTestMessage(venueId)

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-40 w-full" />
        <Skeleton className="h-60 w-full" />
      </div>
    )
  }

  if (!settings) return null

  const handleToggle = (
    field: keyof Omit<typeof settings, 'isConnected'>,
    value: boolean,
  ) => {
    updateMutation.mutate({ ...settings, [field]: value })
  }

  const handleSendTest = () => {
    if (!testPhone.trim()) return
    testMutation.mutate(testPhone.trim())
  }

  return (
    <div className="space-y-6">
      {/* Ana kart */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-100 rounded-lg">
                <MessageCircle className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <CardTitle className="text-base">WhatsApp Bildirimleri</CardTitle>
                <CardDescription>
                  Müşterilerinize WhatsApp üzerinden otomatik bildirim gönderin.
                </CardDescription>
              </div>
            </div>
            <div className="flex items-center gap-3">
              {/* Bağlantı durumu */}
              {settings.isConnected ? (
                <Badge variant="outline" className="text-green-600 border-green-300 bg-green-50 gap-1.5">
                  <Wifi className="h-3 w-3" />
                  Bağlı
                </Badge>
              ) : (
                <Badge variant="outline" className="text-gray-500 border-gray-300 bg-gray-50 gap-1.5">
                  <WifiOff className="h-3 w-3" />
                  Bağlı Değil
                </Badge>
              )}
              {/* Master toggle */}
              <Switch
                checked={settings.whatsAppEnabled}
                onCheckedChange={(v) => handleToggle('whatsAppEnabled', v)}
                disabled={updateMutation.isPending}
              />
            </div>
          </div>
        </CardHeader>

        <CardContent className="space-y-4">
          {/* Platform bağlantı uyarısı */}
          {!settings.isConnected && (
            <Alert className="border-amber-200 bg-amber-50">
              <AlertCircle className="h-4 w-4 text-amber-600" />
              <AlertDescription className="text-amber-800 text-sm">
                Platform WhatsApp bağlantısı yapılandırılmamış. Sistem yöneticisiyle iletişime geçin.
              </AlertDescription>
            </Alert>
          )}

          {/* WhatsApp kapalıyken fallback bilgisi */}
          {!settings.whatsAppEnabled && (
            <Alert className="border-blue-200 bg-blue-50">
              <Info className="h-4 w-4 text-blue-600" />
              <AlertDescription className="text-blue-800 text-sm">
                WhatsApp kapalı — tüm bildirimler email ile gönderilir.
              </AlertDescription>
            </Alert>
          )}

          <Separator />

          {/* Bildirim türleri */}
          <div className="space-y-1">
            <p className="text-sm font-medium text-gray-700 mb-3">Bildirim Türleri</p>
            <div className="space-y-3">
              {NOTIFICATION_TOGGLES.map((toggle) => (
                <div
                  key={toggle.key}
                  className="flex items-center justify-between py-2 px-3 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  <div>
                    <Label className="text-sm font-medium cursor-pointer">{toggle.label}</Label>
                    <p className="text-xs text-gray-500 mt-0.5">{toggle.description}</p>
                  </div>
                  <Switch
                    checked={settings[toggle.key]}
                    onCheckedChange={(v) => handleToggle(toggle.key, v)}
                    disabled={!settings.whatsAppEnabled || updateMutation.isPending}
                  />
                </div>
              ))}

              {/* Yakında gelecek togglelar — grayed out */}
              {COMING_SOON_TOGGLES.map((toggle) => (
                <div
                  key={toggle.label}
                  className="flex items-center justify-between py-2 px-3 rounded-lg opacity-50 cursor-not-allowed"
                >
                  <div>
                    <div className="flex items-center gap-2">
                      <Label className="text-sm font-medium text-gray-400">{toggle.label}</Label>
                      <Badge variant="secondary" className="text-xs h-4 px-1.5">Yakında</Badge>
                    </div>
                    <p className="text-xs text-gray-400 mt-0.5">{toggle.description}</p>
                  </div>
                  <Switch checked={false} disabled />
                </div>
              ))}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Test mesajı kartı */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Test Mesajı Gönder</CardTitle>
          <CardDescription>
            WhatsApp bağlantısını doğrulamak için kendi numaranıza test mesajı gönderin.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input
              placeholder="+905XXXXXXXXX"
              value={testPhone}
              onChange={(e) => setTestPhone(e.target.value)}
              className="max-w-xs"
              disabled={testMutation.isPending}
            />
            <Button
              variant="outline"
              onClick={handleSendTest}
              disabled={!testPhone.trim() || testMutation.isPending}
              className="gap-2"
            >
              <Send className="h-4 w-4" />
              {testMutation.isPending ? 'Gönderiliyor…' : 'Gönder'}
            </Button>
          </div>
          <p className="text-xs text-gray-500 mt-2">
            E.164 formatı: +905321234567. Twilio yapılandırılmamışsa mesaj loglanır.
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
