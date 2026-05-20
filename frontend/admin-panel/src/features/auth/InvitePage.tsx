import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import api from '@/lib/api'
import { getApiErrorMessage, mapAuthResultToUser, type AuthResultPayload } from '@/lib/mapAuthResult'
import { useAuthStore } from '@/stores/authStore'
import { AuthPageShell } from './components/AuthPageShell'
import { passwordFieldSchema } from './schemas/passwordSchema'

interface InvitationPreview {
  email: string
  tenantName: string
  invitedBy: string
  role: string
  invitedAt: string
  expiresAt: string
}

const acceptInviteSchema = z
  .object({
    firstName: z.string().min(1, 'Ad zorunludur'),
    lastName: z.string().min(1, 'Soyad zorunludur'),
    password: passwordFieldSchema,
    confirmPassword: z.string().min(1, 'Şifre tekrarı zorunludur'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Şifreler eşleşmiyor',
    path: ['confirmPassword'],
  })

type AcceptInviteForm = z.infer<typeof acceptInviteSchema>

export function InvitePage() {
  const { token } = useParams<{ token: string }>()
  const navigate = useNavigate()
  const login = useAuthStore((state) => state.login)

  const [preview, setPreview] = useState<InvitationPreview | null>(null)
  const [previewError, setPreviewError] = useState<string | null>(null)
  const [previewLoading, setPreviewLoading] = useState(true)
  const [acceptLoading, setAcceptLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AcceptInviteForm>({
    resolver: zodResolver(acceptInviteSchema),
  })

  useEffect(() => {
    if (!token) {
      setPreviewError('Davet bağlantısı geçersiz.')
      setPreviewLoading(false)
      return
    }

    let cancelled = false

    const loadPreview = async () => {
      try {
        const response = await api.get<InvitationPreview>(`/api/v1/invite/${token}`)
        if (!cancelled) {
          setPreview(response.data)
        }
      } catch (error: unknown) {
        if (!cancelled) {
          setPreviewError(getApiErrorMessage(error, 'Davet bulunamadı veya süresi dolmuş'))
        }
      } finally {
        if (!cancelled) {
          setPreviewLoading(false)
        }
      }
    }

    void loadPreview()

    return () => {
      cancelled = true
    }
  }, [token])

  const onSubmit = async (data: AcceptInviteForm) => {
    if (!token) return

    setAcceptLoading(true)
    try {
      const response = await api.post<AuthResultPayload>(`/api/v1/invite/${token}/accept`, {
        firstName: data.firstName,
        lastName: data.lastName,
        password: data.password,
        confirmPassword: data.confirmPassword,
      })
      const payload = response.data
      login(
        mapAuthResultToUser(payload),
        payload.tokens.accessToken,
        payload.tokens.refreshToken
      )
      toast.success('Davet kabul edildi')
      navigate('/dashboard')
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, 'Davet kabul edilemedi'))
    } finally {
      setAcceptLoading(false)
    }
  }

  if (previewLoading) {
    return (
      <AuthPageShell title="Davet" description="Davet bilgileri yükleniyor...">
        <div className="flex justify-center py-8">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      </AuthPageShell>
    )
  }

  if (previewError || !preview) {
    return (
      <AuthPageShell title="Davet" description="Davet geçersiz">
        <p className="text-center text-sm text-destructive">{previewError ?? 'Davet bulunamadı'}</p>
      </AuthPageShell>
    )
  }

  const isExpired = new Date(preview.expiresAt) < new Date()

  return (
    <AuthPageShell
      title="Ekibe Katılın"
      description={`${preview.tenantName} sizi davet etti`}
    >
      <div className="mb-4 space-y-1 rounded-md bg-muted/50 p-3 text-sm">
        <p>
          <span className="text-muted-foreground">Email: </span>
          {preview.email}
        </p>
        <p>
          <span className="text-muted-foreground">Rol: </span>
          {preview.role}
        </p>
        <p>
          <span className="text-muted-foreground">Davet eden: </span>
          {preview.invitedBy}
        </p>
        <p>
          <span className="text-muted-foreground">Son tarih: </span>
          {format(new Date(preview.expiresAt), 'dd MMM yyyy HH:mm', { locale: tr })}
        </p>
        {isExpired && (
          <p className="font-medium text-destructive">Bu davetin süresi dolmuş.</p>
        )}
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <Label htmlFor="firstName">Ad *</Label>
            <Input id="firstName" {...register('firstName')} disabled={isExpired} />
            {errors.firstName && (
              <p className="text-sm text-destructive">{errors.firstName.message}</p>
            )}
          </div>
          <div className="space-y-2">
            <Label htmlFor="lastName">Soyad *</Label>
            <Input id="lastName" {...register('lastName')} disabled={isExpired} />
            {errors.lastName && (
              <p className="text-sm text-destructive">{errors.lastName.message}</p>
            )}
          </div>
        </div>

        <div className="space-y-2">
          <Label htmlFor="password">Şifre *</Label>
          <Input
            id="password"
            type="password"
            autoComplete="new-password"
            {...register('password')}
            disabled={isExpired}
          />
          {errors.password && (
            <p className="text-sm text-destructive">{errors.password.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="confirmPassword">Şifre tekrarı *</Label>
          <Input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            {...register('confirmPassword')}
            disabled={isExpired}
          />
          {errors.confirmPassword && (
            <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
          )}
        </div>

        <Button type="submit" className="w-full" disabled={acceptLoading || isExpired}>
          {acceptLoading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Kaydediliyor...
            </>
          ) : (
            'Daveti Kabul Et'
          )}
        </Button>
      </form>
    </AuthPageShell>
  )
}
