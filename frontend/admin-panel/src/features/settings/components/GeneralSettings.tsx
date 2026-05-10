import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Upload, Image as ImageIcon } from 'lucide-react'
import { toast } from 'sonner'

const generalSettingsSchema = z.object({
  businessName: z.string().min(1, 'İşletme adı zorunludur'),
  address: z.string().optional(),
  phone: z.string().optional(),
  timezone: z.string(),
  language: z.string(),
})

type GeneralSettingsForm = z.infer<typeof generalSettingsSchema>

export function GeneralSettings() {
  const [logoPreview, setLogoPreview] = useState<string | null>(null)
  const [isUploading, setIsUploading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<GeneralSettingsForm>({
    resolver: zodResolver(generalSettingsSchema),
    defaultValues: {
      businessName: 'Demo Restoran',
      address: '',
      phone: '',
      timezone: 'Europe/Istanbul',
      language: 'tr',
    },
  })

  const handleLogoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    setIsUploading(true)
    try {
      // TODO: R2 presigned URL flow
      // 1. GET /api/v1/tenant/me/logo -> presigned URL
      // 2. PUT to presigned URL (direct to R2)
      // 3. POST /api/v1/tenant/me/logo/confirm

      const preview = URL.createObjectURL(file)
      setLogoPreview(preview)
      toast.success('Logo yüklendi')
    } catch (error) {
      toast.error('Logo yüklenemedi')
    } finally {
      setIsUploading(false)
    }
  }

  const onSubmit = (data: GeneralSettingsForm) => {
    console.log('Settings:', data)
    toast.success('Ayarlar kaydedildi')
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Logo</CardTitle>
          <CardDescription>İşletme logonuzu yükleyin</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-4">
            <div className="flex h-24 w-24 items-center justify-center rounded-lg border-2 border-dashed border-muted-foreground/25 bg-muted">
              {logoPreview ? (
                <img src={logoPreview} alt="Logo" className="h-full w-full rounded-lg object-cover" />
              ) : (
                <ImageIcon className="h-8 w-8 text-muted-foreground" />
              )}
            </div>
            <div>
              <input
                type="file"
                accept="image/*"
                onChange={handleLogoUpload}
                className="hidden"
                id="logo-upload"
              />
              <Button asChild disabled={isUploading}>
                <label htmlFor="logo-upload" className="cursor-pointer">
                  <Upload className="mr-2 h-4 w-4" />
                  {isUploading ? 'Yükleniyor...' : 'Logo Yükle'}
                </label>
              </Button>
              <p className="mt-2 text-sm text-muted-foreground">
                PNG, JPG veya SVG. Maksimum 2MB.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>İşletme Bilgileri</CardTitle>
          <CardDescription>Genel işletme bilgilerinizi düzenleyin</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="businessName">İşletme Adı *</Label>
              <Input id="businessName" {...register('businessName')} />
              {errors.businessName && (
                <p className="text-sm text-destructive">{errors.businessName.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="address">Adres</Label>
              <Textarea id="address" {...register('address')} rows={3} />
            </div>

            <div className="space-y-2">
              <Label htmlFor="phone">Telefon</Label>
              <Input id="phone" type="tel" {...register('phone')} />
            </div>

            <div className="space-y-2">
              <Label>Zaman Dilimi</Label>
              <Select value={watch('timezone')} onValueChange={(value) => setValue('timezone', value)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Europe/Istanbul">İstanbul (UTC+3)</SelectItem>
                  <SelectItem value="Europe/Athens">Atina (UTC+2)</SelectItem>
                  <SelectItem value="Europe/London">Londra (UTC+0)</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Dil</Label>
              <Select value={watch('language')} onValueChange={(value) => setValue('language', value)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="tr">Türkçe</SelectItem>
                  <SelectItem value="en" disabled>
                    English (Yakında)
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            <Button type="submit">Kaydet</Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
