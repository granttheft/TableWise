import { useState, useRef } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import api from '@/lib/api'

const registerSchema = z.object({
  tenantName: z.string().min(2, 'İşletme adı en az 2 karakter olmalı'),
  email: z.string().email('Geçerli bir email adresi girin'),
  password: z
    .string()
    .min(8, 'Şifre en az 8 karakter olmalı')
    .regex(/[A-Z]/, 'En az bir büyük harf içermeli')
    .regex(/[a-z]/, 'En az bir küçük harf içermeli')
    .regex(/[0-9]/, 'En az bir rakam içermeli'),
  confirmPassword: z.string(),
  acceptTerms: z.boolean().refine((val) => val === true, {
    message: 'KVKK onayı gerekli',
  }),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Şifreler eşleşmiyor',
  path: ['confirmPassword'],
})

type RegisterForm = z.infer<typeof registerSchema>

export function RegisterPage() {
  const navigate = useNavigate()
  const [isLoading, setIsLoading] = useState(false)
  const submitLockRef = useRef(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
  })

  const onSubmit = async (data: RegisterForm) => {
    if (submitLockRef.current) return
    submitLockRef.current = true
    setIsLoading(true)
    try {
      const business = data.tenantName.trim()
      const spaceIdx = business.indexOf(' ')
      let firstName =
        spaceIdx > 0 ? business.slice(0, spaceIdx).slice(0, 50) : business.slice(0, 50)
      if (firstName.length < 2) {
        firstName = 'Yönetici'
      }
      const lastNameRaw =
        spaceIdx > 0 ? business.slice(spaceIdx + 1).trim() : 'Yetkilisi'
      const lastName = lastNameRaw.length >= 2 ? lastNameRaw.slice(0, 50) : 'Yetkilisi'

      await api.post('/api/v1/auth/register', {
        businessName: business,
        email: data.email,
        password: data.password,
        confirmPassword: data.confirmPassword,
        firstName,
        lastName,
        acceptTerms: data.acceptTerms,
      })

      toast.success('Kayıt başarılı! Email doğrulama linki gönderildi.')
      navigate('/login')
    } catch (error: unknown) {
      const status = (error as { response?: { status?: number; data?: { detail?: string; message?: string } } })
        ?.response?.status
      if (status === 429) {
        toast.error(
          'Çok fazla istek gönderildi. Lütfen yaklaşık bir dakika bekleyip tekrar deneyin.'
        )
        return
      }
      const message =
        (error as { response?: { data?: { detail?: string; message?: string } } })?.response?.data
          ?.detail ||
        (error as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Kayıt başarısız'
      toast.error(message)
    } finally {
      setIsLoading(false)
      submitLockRef.current = false
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary via-primary/90 to-accent/20 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <div className="mx-auto mb-4 h-12 w-12 rounded-lg bg-gradient-to-br from-primary to-accent" />
          <CardTitle className="text-2xl">Tablewise</CardTitle>
          <CardDescription>Ücretsiz 14 gün deneme başlatın</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="tenantName">İşletme Adı</Label>
              <Input
                id="tenantName"
                placeholder="Restoran/Mekan Adı"
                {...register('tenantName')}
              />
              {errors.tenantName && (
                <p className="text-sm text-destructive">{errors.tenantName.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="ornek@email.com"
                {...register('email')}
              />
              {errors.email && (
                <p className="text-sm text-destructive">{errors.email.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">Şifre</Label>
              <Input
                id="password"
                type="password"
                {...register('password')}
              />
              {errors.password && (
                <p className="text-sm text-destructive">{errors.password.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirmPassword">Şifre Tekrar</Label>
              <Input
                id="confirmPassword"
                type="password"
                {...register('confirmPassword')}
              />
              {errors.confirmPassword && (
                <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
              )}
            </div>

            <div className="flex items-center space-x-2">
              <input
                id="acceptTerms"
                type="checkbox"
                className="h-4 w-4"
                {...register('acceptTerms')}
              />
              <label htmlFor="acceptTerms" className="text-sm">
                <a href="#" className="text-accent hover:underline">
                  KVKK
                </a>{' '}
                ve{' '}
                <a href="#" className="text-accent hover:underline">
                  Kullanım Koşulları
                </a>
                'nı kabul ediyorum
              </label>
            </div>
            {errors.acceptTerms && (
              <p className="text-sm text-destructive">{errors.acceptTerms.message}</p>
            )}

            <Button type="submit" className="w-full" disabled={isLoading}>
              {isLoading ? 'Kayıt oluşturuluyor...' : 'Kayıt Ol'}
            </Button>

            <p className="text-center text-sm text-muted-foreground">
              Zaten hesabınız var mı?{' '}
              <Link to="/login" className="text-accent hover:underline">
                Giriş yap
              </Link>
            </p>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
