import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { CheckCircle, Loader2, XCircle } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import api from '@/lib/api'
import { getApiErrorMessage } from '@/lib/mapAuthResult'
import { AuthPageShell } from './components/AuthPageShell'

const resendSchema = z.object({
  email: z.string().email('Geçerli bir email adresi girin'),
})

type ResendForm = z.infer<typeof resendSchema>

type VerifyState = 'loading' | 'success' | 'error'

export function VerifyEmailPage() {
  const { token } = useParams<{ token: string }>()
  const [state, setState] = useState<VerifyState>('loading')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [resendLoading, setResendLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResendForm>({
    resolver: zodResolver(resendSchema),
  })

  useEffect(() => {
    if (!token) {
      setState('error')
      setErrorMessage('Doğrulama bağlantısı geçersiz.')
      return
    }

    let cancelled = false

    const verify = async () => {
      try {
        await api.post('/api/v1/auth/verify-email', { token })
        if (!cancelled) {
          setState('success')
        }
      } catch (error: unknown) {
        if (!cancelled) {
          setState('error')
          setErrorMessage(getApiErrorMessage(error, 'Link geçersiz veya süresi dolmuş'))
        }
      }
    }

    void verify()

    return () => {
      cancelled = true
    }
  }, [token])

  const onResendSubmit = async (_data: ResendForm) => {
    setResendLoading(true)
    try {
      setErrorMessage(
        'Yeni doğrulama e-postası için lütfen giriş yapın veya destek ekibiyle iletişime geçin.'
      )
    } finally {
      setResendLoading(false)
    }
  }

  if (state === 'loading') {
    return (
      <AuthPageShell title="Email Doğrulama" description="Email adresiniz doğrulanıyor...">
        <div className="flex flex-col items-center gap-4 py-6">
          <Loader2 className="h-10 w-10 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground">Lütfen bekleyin...</p>
        </div>
      </AuthPageShell>
    )
  }

  if (state === 'success') {
    return (
      <AuthPageShell title="Email Doğrulandı" description="Hesabınız aktifleştirildi">
        <div className="flex flex-col items-center gap-4 py-4">
          <CheckCircle className="h-16 w-16 text-green-500" />
          <p className="text-center text-sm">Email doğrulandı. Artık giriş yapabilirsiniz.</p>
          <Button asChild className="w-full">
            <Link to="/login">Giriş Yap</Link>
          </Button>
        </div>
      </AuthPageShell>
    )
  }

  return (
    <AuthPageShell title="Doğrulama Başarısız" description="Link geçersiz veya süresi dolmuş">
      <div className="flex flex-col items-center gap-4">
        <XCircle className="h-16 w-16 text-destructive" />
        <p className="text-center text-sm text-destructive">
          {errorMessage ?? 'Link geçersiz veya süresi dolmuş'}
        </p>

        <form onSubmit={handleSubmit(onResendSubmit)} className="w-full space-y-3 border-t pt-4">
          <p className="text-center text-sm text-muted-foreground">
            Kayıtlı email adresinizi girin; hesabınızla ilgili yönlendirme alın.
          </p>
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" placeholder="ornek@email.com" {...register('email')} />
            {errors.email && (
              <p className="text-sm text-destructive">{errors.email.message}</p>
            )}
          </div>
          <Button type="submit" variant="outline" className="w-full" disabled={resendLoading}>
            {resendLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Gönderiliyor...
              </>
            ) : (
              'Tekrar Gönder'
            )}
          </Button>
        </form>

        <Button asChild className="w-full">
          <Link to="/login">Giriş Yap</Link>
        </Button>
      </div>
    </AuthPageShell>
  )
}
