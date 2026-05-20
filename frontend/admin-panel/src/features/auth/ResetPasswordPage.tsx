import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import api from '@/lib/api'
import { getApiErrorMessage } from '@/lib/mapAuthResult'
import { AuthPageShell } from './components/AuthPageShell'
import { passwordFieldSchema } from './schemas/passwordSchema'

const resetPasswordSchema = z
  .object({
    password: passwordFieldSchema,
    confirmPassword: z.string().min(1, 'Şifre tekrarı zorunludur'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Şifreler eşleşmiyor',
    path: ['confirmPassword'],
  })

type ResetPasswordForm = z.infer<typeof resetPasswordSchema>

export function ResetPasswordPage() {
  const { token } = useParams<{ token: string }>()
  const navigate = useNavigate()
  const [tokenError, setTokenError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordForm>({
    resolver: zodResolver(resetPasswordSchema),
  })

  const onSubmit = async (data: ResetPasswordForm) => {
    if (!token) {
      setTokenError('Geçersiz sıfırlama bağlantısı')
      return
    }

    setIsLoading(true)
    setTokenError(null)
    try {
      await api.post('/api/v1/auth/reset-password', {
        token,
        newPassword: data.password,
      })
      toast.success('Şifreniz güncellendi')
      navigate('/login')
    } catch (error: unknown) {
      const message = getApiErrorMessage(error, 'Şifre sıfırlanamadı')
      setTokenError(message)
    } finally {
      setIsLoading(false)
    }
  }

  if (!token) {
    return (
      <AuthPageShell title="Şifre Sıfırla" description="Bağlantı geçersiz">
        <p className="text-center text-sm text-destructive">Sıfırlama bağlantısı eksik veya hatalı.</p>
        <Button asChild className="mt-4 w-full">
          <Link to="/forgot-password">Tekrar sıfırlama linki</Link>
        </Button>
      </AuthPageShell>
    )
  }

  return (
    <AuthPageShell title="Şifre Sıfırla" description="Yeni şifrenizi belirleyin">
      {tokenError ? (
        <div className="mb-4 space-y-3 rounded-md border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
          <p>{tokenError}</p>
          <Button asChild variant="outline" className="w-full">
            <Link to="/forgot-password">Tekrar sıfırlama linki</Link>
          </Button>
        </div>
      ) : null}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="space-y-2">
          <Label htmlFor="password">Yeni şifre</Label>
          <Input id="password" type="password" autoComplete="new-password" {...register('password')} />
          {errors.password && (
            <p className="text-sm text-destructive">{errors.password.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="confirmPassword">Şifre tekrarı</Label>
          <Input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            {...register('confirmPassword')}
          />
          {errors.confirmPassword && (
            <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
          )}
        </div>

        <Button type="submit" className="w-full" disabled={isLoading}>
          {isLoading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Kaydediliyor...
            </>
          ) : (
            'Şifreyi Güncelle'
          )}
        </Button>

        <p className="text-center text-sm text-muted-foreground">
          <Link to="/login" className="text-accent hover:underline">
            Giriş sayfasına dön
          </Link>
        </p>
      </form>
    </AuthPageShell>
  )
}
