import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Checkbox } from '@/components/ui/checkbox'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Eye, EyeOff, RefreshCw, Key, Webhook, Info } from 'lucide-react'
import { toast } from 'sonner'

export function IntegrationSettings() {
  const [currentPlan] = useState('Starter') // TODO: Get from auth store/API
  const isBusinessOrAbove = currentPlan === 'Business' || currentPlan === 'Enterprise'

  const [apiKey] = useState('tw_live_1234567890abcdef')
  const [showApiKey, setShowApiKey] = useState(false)
  const [webhookUrl, setWebhookUrl] = useState('')
  const [webhookSecret, setWebhookSecret] = useState('')
  const [webhookEvents, setWebhookEvents] = useState({
    'reservation.created': true,
    'reservation.updated': true,
    'reservation.cancelled': true,
    'reservation.completed': false,
    'reservation.noshow': false,
    'deposit.paid': false,
    'deposit.refunded': false,
  })

  const handleRegenerateApiKey = () => {
    if (!confirm('API key yenilenecek. Mevcut entegrasyonlarınız çalışmayı durduracak. Devam etmek istiyor musunuz?')) {
      return
    }
    toast.success('API key yenilendi')
  }

  const toggleWebhookEvent = (event: string) => {
    setWebhookEvents({
      ...webhookEvents,
      [event]: !webhookEvents[event as keyof typeof webhookEvents],
    })
  }

  const handleSaveWebhook = () => {
    console.log('Webhook settings:', { webhookUrl, webhookSecret, webhookEvents })
    toast.success('Webhook ayarları kaydedildi')
  }

  return (
    <div className="space-y-6">
      {!isBusinessOrAbove && (
        <Alert>
          <Info className="h-4 w-4" />
          <AlertDescription>
            API ve Webhook özellikleri Business plandan itibaren kullanılabilir.{' '}
            <a href="/subscription" className="font-medium underline">
              Planınızı yükseltin
            </a>
          </AlertDescription>
        </Alert>
      )}

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <Key className="h-5 w-5" />
                API Key
                {!isBusinessOrAbove && <Badge variant="secondary">Business+</Badge>}
              </CardTitle>
              <CardDescription>Harici entegrasyonlar için API key'iniz</CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {isBusinessOrAbove ? (
            <>
              <div className="flex items-center gap-2">
                <div className="relative flex-1">
                  <Input
                    value={apiKey}
                    type={showApiKey ? 'text' : 'password'}
                    readOnly
                    className="pr-10"
                  />
                  <Button
                    variant="ghost"
                    size="icon"
                    className="absolute right-0 top-0"
                    onClick={() => setShowApiKey(!showApiKey)}
                  >
                    {showApiKey ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </Button>
                </div>
                <Button variant="outline" onClick={handleRegenerateApiKey}>
                  <RefreshCw className="mr-2 h-4 w-4" />
                  Yenile
                </Button>
              </div>

              <Alert>
                <Info className="h-4 w-4" />
                <AlertDescription>
                  API dokümantasyonuna{' '}
                  <a href="https://docs.tablewise.com.tr/api" className="font-medium underline">
                    buradan
                  </a>{' '}
                  ulaşabilirsiniz.
                </AlertDescription>
              </Alert>
            </>
          ) : (
            <div className="rounded-lg border border-dashed p-8 text-center">
              <Key className="mx-auto mb-2 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">API erişimi Business plandan itibaren</p>
              <Button variant="link" asChild>
                <Link to="/subscription">Planınızı yükseltin</Link>
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Webhook className="h-5 w-5" />
            Webhook
            {!isBusinessOrAbove && <Badge variant="secondary">Business+</Badge>}
          </CardTitle>
          <CardDescription>
            Rezervasyon olayları için webhook URL'nizi tanımlayın
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {isBusinessOrAbove ? (
            <>
              <div className="space-y-2">
                <Label>Webhook URL</Label>
                <Input
                  value={webhookUrl}
                  onChange={(e) => setWebhookUrl(e.target.value)}
                  placeholder="https://your-domain.com/webhook/tablewise"
                />
              </div>

              <div className="space-y-2">
                <Label>Webhook Secret</Label>
                <Input
                  value={webhookSecret}
                  onChange={(e) => setWebhookSecret(e.target.value)}
                  placeholder="Signature doğrulama için gizli anahtar"
                  type="password"
                />
                <p className="text-sm text-muted-foreground">
                  Her webhook isteği bu secret ile HMAC-SHA256 imzalanır
                </p>
              </div>

              <div className="space-y-3">
                <Label>Webhook Event'leri</Label>
                <div className="space-y-2">
                  {Object.entries(webhookEvents).map(([event, enabled]) => (
                    <div key={event} className="flex items-center space-x-2">
                      <Checkbox
                        id={event}
                        checked={enabled}
                        onCheckedChange={() => toggleWebhookEvent(event)}
                      />
                      <label htmlFor={event} className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
                        {event}
                      </label>
                    </div>
                  ))}
                </div>
              </div>

              <Button onClick={handleSaveWebhook}>Kaydet</Button>
            </>
          ) : (
            <div className="rounded-lg border border-dashed p-8 text-center">
              <Webhook className="mx-auto mb-2 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">Webhook entegrasyonu Business plandan itibaren</p>
              <Button variant="link" asChild>
                <Link to="/subscription">Planınızı yükseltin</Link>
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
