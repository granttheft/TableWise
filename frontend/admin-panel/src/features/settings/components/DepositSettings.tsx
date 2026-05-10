import { useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Slider } from '@/components/ui/slider'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { CreditCard, Info } from 'lucide-react'
import { toast } from 'sonner'

export function DepositSettings() {
  const [depositEnabled, setDepositEnabled] = useState(false)
  const [depositType, setDepositType] = useState<'fixed' | 'perPerson'>('fixed')
  const [depositAmount, setDepositAmount] = useState(100)
  const [refundPolicy, setRefundPolicy] = useState<'full' | 'partial' | 'none'>('full')
  const [refundHours, setRefundHours] = useState(24)
  const [partialRefundPercent, setPartialRefundPercent] = useState(50)

  const handleSave = () => {
    console.log('Deposit settings:', {
      depositEnabled,
      depositType,
      depositAmount,
      refundPolicy,
      refundHours,
      partialRefundPercent,
    })
    toast.success('Kapora ayarları kaydedildi')
  }

  return (
    <div className="space-y-6">
      <Alert>
        <Info className="h-4 w-4" />
        <AlertDescription>
          Kapora modülü İyzico entegrasyonu ile çalışır. Ödemeler güvenli bir şekilde işlenir ve
          otomatik iade politikanız uygulanır.
        </AlertDescription>
      </Alert>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Kapora Modülü</CardTitle>
              <CardDescription>Rezervasyonlar için kapora zorunluluğu</CardDescription>
            </div>
            <Switch checked={depositEnabled} onCheckedChange={setDepositEnabled} />
          </div>
        </CardHeader>
      </Card>

      {depositEnabled && (
        <>
          <Card>
            <CardHeader>
              <CardTitle>Kapora Tutarı</CardTitle>
              <CardDescription>Müşterilerden alınacak kapora miktarı</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label>Kapora Tipi</Label>
                <Select value={depositType} onValueChange={(value: any) => setDepositType(value)}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="fixed">Sabit Tutar</SelectItem>
                    <SelectItem value="perPerson">Kişi Başı</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Tutar (₺)</Label>
                <Input
                  type="number"
                  value={depositAmount}
                  onChange={(e) => setDepositAmount(parseInt(e.target.value) || 0)}
                  min={10}
                  max={10000}
                />
                <p className="text-sm text-muted-foreground">
                  {depositType === 'perPerson'
                    ? 'Her kişi için alınacak kapora tutarı'
                    : 'Rezervasyon başına sabit kapora tutarı'}
                </p>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>İade Politikası</CardTitle>
              <CardDescription>İptal durumunda kapora iade koşulları</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label>İade Tipi</Label>
                <Select value={refundPolicy} onValueChange={(value: any) => setRefundPolicy(value)}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="full">Tam İade</SelectItem>
                    <SelectItem value="partial">Kısmi İade</SelectItem>
                    <SelectItem value="none">İade Yok</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {refundPolicy !== 'none' && (
                <div className="space-y-2">
                  <Label>Minimum İptal Süresi (Saat)</Label>
                  <Input
                    type="number"
                    value={refundHours}
                    onChange={(e) => setRefundHours(parseInt(e.target.value) || 0)}
                    min={1}
                    max={168}
                  />
                  <p className="text-sm text-muted-foreground">
                    Rezervasyondan kaç saat önce iptal edilirse iade yapılır
                  </p>
                </div>
              )}

              {refundPolicy === 'partial' && (
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <Label>İade Oranı: %{partialRefundPercent}</Label>
                  </div>
                  <Slider
                    value={[partialRefundPercent]}
                    onValueChange={(value) => setPartialRefundPercent(value[0])}
                    min={10}
                    max={90}
                    step={10}
                  />
                  <p className="text-sm text-muted-foreground">
                    Kapora tutarının %{partialRefundPercent}'i iade edilir
                  </p>
                </div>
              )}
            </CardContent>
          </Card>

          <Alert>
            <CreditCard className="h-4 w-4" />
            <AlertDescription>
              Kapora ödemeleri İyzico üzerinden alınır. İyzico hesabınızı Entegrasyon sekmesinden
              bağlayın.
            </AlertDescription>
          </Alert>
        </>
      )}

      {!depositEnabled && (
        <Alert>
          <Info className="h-4 w-4" />
          <AlertDescription>
            Kapora modülü pasif. Kural motorundaki "Deposit Required" kuralları çalışmayacaktır.
          </AlertDescription>
        </Alert>
      )}

      <Button onClick={handleSave} disabled={!depositEnabled}>
        Kaydet
      </Button>
    </div>
  )
}
