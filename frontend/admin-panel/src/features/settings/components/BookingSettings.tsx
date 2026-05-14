import { useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Switch } from '@/components/ui/switch'
import { Copy, Plus, GripVertical, X, QrCode, Code } from 'lucide-react'
import { toast } from 'sonner'

interface CustomField {
  id: string
  label: string
  type: 'text' | 'textarea' | 'select' | 'checkbox'
  placeholder?: string
  options?: string[]
  isRequired: boolean
  order: number
}

export function BookingSettings() {
  const [bookingUrl] = useState('https://tablewise.com.tr/rezervasyon/demo-restoran')
  const [customFields, setCustomFields] = useState<CustomField[]>([
    {
      id: '1',
      label: 'Özel Tarih (Doğum günü vb.)',
      type: 'checkbox',
      isRequired: false,
      order: 1,
    },
  ])

  const handleCopyUrl = () => {
    navigator.clipboard.writeText(bookingUrl)
    toast.success('Booking URL kopyalandı')
  }

  const handleCopyEmbed = () => {
    const embedCode = `<iframe src="${bookingUrl}" width="100%" height="800" frameborder="0"></iframe>`
    navigator.clipboard.writeText(embedCode)
    toast.success('Embed kodu kopyalandı')
  }

  const handleShowQR = () => {
    toast.info('QR kod oluşturuluyor...')
    // TODO: QR code generation modal
  }

  const addCustomField = () => {
    const newField: CustomField = {
      id: Date.now().toString(),
      label: 'Yeni Alan',
      type: 'text',
      placeholder: '',
      isRequired: false,
      order: customFields.length + 1,
    }
    setCustomFields([...customFields, newField])
  }

  const removeCustomField = (id: string) => {
    setCustomFields(customFields.filter((f) => f.id !== id))
  }

  const updateCustomField = (id: string, updates: Partial<CustomField>) => {
    setCustomFields(customFields.map((f) => (f.id === id ? { ...f, ...updates } : f)))
  }

  const handleSave = () => {
    console.log('Custom fields:', customFields)
    toast.success('Booking ayarları kaydedildi')
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Booking Linki</CardTitle>
          <CardDescription>Müşterilerinizin rezervasyon yapabileceği link</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-2">
            <Input value={bookingUrl} readOnly className="flex-1" />
            <Button variant="outline" onClick={handleCopyUrl}>
              <Copy className="mr-2 h-4 w-4" />
              Kopyala
            </Button>
          </div>

          <div className="flex gap-2">
            <Button variant="outline" onClick={handleShowQR}>
              <QrCode className="mr-2 h-4 w-4" />
              QR Kod Oluştur
            </Button>
            <Button variant="outline" onClick={handleCopyEmbed}>
              <Code className="mr-2 h-4 w-4" />
              Embed Kodu
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Özel Form Alanları</CardTitle>
              <CardDescription>Booking formuna ekstra alanlar ekleyin</CardDescription>
            </div>
            <Button size="sm" onClick={addCustomField}>
              <Plus className="mr-2 h-4 w-4" />
              Alan Ekle
            </Button>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {customFields.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-8 text-center text-muted-foreground">
              <p>Henüz özel alan eklenmedi</p>
              <p className="text-sm">Müşterilerinizden ek bilgi toplamak için özel alanlar ekleyin</p>
            </div>
          ) : (
            customFields.map((field) => (
              <div
                key={field.id}
                className="flex items-start gap-4 rounded-lg border p-4"
              >
                <div className="cursor-move pt-2">
                  <GripVertical className="h-5 w-5 text-muted-foreground" />
                </div>

                <div className="flex-1 space-y-3">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label>Alan Adı</Label>
                      <Input
                        value={field.label}
                        onChange={(e) => updateCustomField(field.id, { label: e.target.value })}
                        placeholder="Örn: Alerjileriniz var mı?"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>Tip</Label>
                      <Select
                        value={field.type}
                        onValueChange={(value: any) => updateCustomField(field.id, { type: value })}
                      >
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="text">Kısa Metin</SelectItem>
                          <SelectItem value="textarea">Uzun Metin</SelectItem>
                          <SelectItem value="select">Seçim Listesi</SelectItem>
                          <SelectItem value="checkbox">Onay Kutusu</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  {(field.type === 'text' || field.type === 'textarea') && (
                    <div className="space-y-2">
                      <Label>Placeholder</Label>
                      <Input
                        value={field.placeholder || ''}
                        onChange={(e) => updateCustomField(field.id, { placeholder: e.target.value })}
                        placeholder="İpucu metni..."
                      />
                    </div>
                  )}

                  {field.type === 'select' && (
                    <div className="space-y-2">
                      <Label>Seçenekler (virgülle ayırın)</Label>
                      <Input
                        value={field.options?.join(', ') || ''}
                        onChange={(e) =>
                          updateCustomField(field.id, { options: e.target.value.split(',').map((s) => s.trim()) })
                        }
                        placeholder="Örn: Evet, Hayır, Emin Değilim"
                      />
                    </div>
                  )}

                  <div className="flex items-center space-x-2">
                    <Switch
                      checked={field.isRequired}
                      onCheckedChange={(checked) => updateCustomField(field.id, { isRequired: checked })}
                    />
                    <Label className="cursor-pointer">Zorunlu alan</Label>
                  </div>
                </div>

                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => removeCustomField(field.id)}
                  className="text-destructive"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            ))
          )}

          <Button onClick={handleSave} className="w-full">
            Kaydet
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
