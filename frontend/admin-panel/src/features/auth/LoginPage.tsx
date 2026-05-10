import { useState } from 'react'
import { useNavigate, useSearchParams, Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { useAuthStore } from '@/stores/authStore'
import api from '@/lib/api'
import { getApiErrorMessage, mapAuthResultToUser, type AuthResultPayload } from '@/lib/mapAuthResult'

const loginSchema = z.object({
  email: z.string().email('Geçerli bir email adresi girin'),
  password: z.string().min(1, 'Şifre gerekli'),
})

type LoginForm = z.infer<typeof loginSchema>

const isDevelopment = import.meta.env.DEV

export function LoginPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const login = useAuthStore((state) => state.login)
  const [isLoading, setIsLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
  })

  const handleDevLogin = () => {
    login(
      {
        id: 'dev-user-1',
        email: 'demo@tablewise.com.tr',
        name: 'Demo Kullanıcı',
        role: 'Owner',
        tenantId: 'dev-tenant-1',
        tenantName: 'Demo Restoran',
        planName: 'Pro',
        planStatus: 'Active',
      },
      'dev-access-token',
      'dev-refresh-token'
    )
    toast.success('Development mode - Giriş yapıldı')
    const redirect = searchParams.get('redirect') || '/dashboard'
    navigate(redirect)
  }

  const onSubmit = async (data: LoginForm) => {
    setIsLoading(true)
    try {
      const response = await api.post<AuthResultPayload>('/api/v1/auth/login', {
        email: data.email,
        password: data.password,
        rememberMe: false,
      })
      const payload = response.data
      const storeUser = mapAuthResultToUser(payload)

      login(storeUser, payload.tokens.accessToken, payload.tokens.refreshToken)

      const redirect = searchParams.get('redirect') || '/dashboard'
      navigate(redirect)

      toast.success('Giriş başarılı')
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, 'Giriş başarısız'))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary via-primary/90 to-accent/20 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <div className="mx-auto mb-4 h-12 w-12 rounded-lg bg-gradient-to-br from-primary to-accent" />
          <CardTitle className="text-2xl">Tablewise</CardTitle>
          <CardDescription>Admin paneline giriş yapın</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
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

            <div className="flex items-center justify-between text-sm">
              <Link to="/forgot-password" className="text-accent hover:underline">
                Şifremi unuttum
              </Link>
            </div>

            <Button type="submit" className="w-full" disabled={isLoading}>
              {isLoading ? 'Giriş yapılıyor...' : 'Giriş Yap'}
            </Button>

            {isDevelopment && (
              <>
                <div className="relative my-4">
                  <div className="absolute inset-0 flex items-center">
                    <span className="w-full border-t" />
                  </div>
                  <div className="relative flex justify-center text-xs uppercase">
                    <span className="bg-background px-2 text-muted-foreground">
                      Development Mode
                    </span>
                  </div>
                </div>
                
                <Button
                  type="button"
                  variant="outline"
                  className="w-full border-amber-500 text-amber-500 hover:bg-amber-500 hover:text-white"
                  onClick={handleDevLogin}
                >
                  🚀 Dev Login (Backend API Olmadan Test)
                </Button>
              </>
            )}

            <p className="text-center text-sm text-muted-foreground">
              Hesabınız yok mu?{' '}
              <Link to="/register" className="text-accent hover:underline">
                Kayıt ol
              </Link>
            </p>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
