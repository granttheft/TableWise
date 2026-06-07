import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { UserPlus, Users } from 'lucide-react'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import {
  usePlatformUsers,
  useInvitePlatformUser,
  useUpdatePlatformUserRole,
  useTogglePlatformUserActive,
} from '@/hooks/usePlatformUsers'
import { useAuth } from '@/hooks/useAuth'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import type { PlatformRole, PlatformUserDto } from '@/types/api'

const ROLE_LABELS: Record<PlatformRole, string> = {
  SuperAdmin: 'Süper Admin',
  Marketing: 'Pazarlama',
  Finance: 'Finans',
}

const ROLE_VARIANTS: Record<PlatformRole, 'default' | 'secondary' | 'outline'> = {
  SuperAdmin: 'default',
  Marketing: 'secondary',
  Finance: 'outline',
}

const inviteSchema = z
  .object({
    email: z.string().email('Geçerli e-posta girin'),
    fullName: z.string().min(2, 'En az 2 karakter'),
    role: z.enum(['SuperAdmin', 'Marketing', 'Finance']),
    password: z.string().min(8, 'En az 8 karakter'),
    passwordConfirm: z.string(),
  })
  .refine((d) => d.password === d.passwordConfirm, {
    message: 'Şifreler eşleşmiyor',
    path: ['passwordConfirm'],
  })

type InviteForm = z.infer<typeof inviteSchema>

function RoleSelect({ user }: { user: PlatformUserDto }) {
  const { mutate, isPending } = useUpdatePlatformUserRole(user.id)

  return (
    <Select
      value={user.role}
      onValueChange={(v) =>
        mutate(
          { newRole: v as PlatformRole },
          { onSuccess: () => toast.success('Rol güncellendi.'), onError: () => toast.error('Hata oluştu.') }
        )
      }
      disabled={isPending}
    >
      <SelectTrigger className="h-8 w-36 text-xs">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value="SuperAdmin">Süper Admin</SelectItem>
        <SelectItem value="Marketing">Pazarlama</SelectItem>
        <SelectItem value="Finance">Finans</SelectItem>
      </SelectContent>
    </Select>
  )
}

function ToggleActiveButton({ user, currentUserEmail }: { user: PlatformUserDto; currentUserEmail?: string }) {
  const { mutate, isPending } = useTogglePlatformUserActive(user.id)
  const isSelf = user.email === currentUserEmail

  return (
    <Button
      size="sm"
      variant={user.isActive ? 'outline' : 'default'}
      disabled={isPending || isSelf}
      title={isSelf ? 'Kendinizi deaktif edemezsiniz' : undefined}
      onClick={() =>
        mutate(undefined, {
          onSuccess: () => toast.success(user.isActive ? 'Kullanıcı deaktif edildi.' : 'Kullanıcı aktif edildi.'),
          onError: () => toast.error('Hata oluştu.'),
        })
      }
    >
      {user.isActive ? 'Deaktif Et' : 'Aktif Et'}
    </Button>
  )
}

export function TeamPage() {
  const { data: users, isLoading } = usePlatformUsers()
  const { isSuperAdmin, user: currentUser } = useAuth()
  const [inviteOpen, setInviteOpen] = useState(false)
  const invite = useInvitePlatformUser()

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm<InviteForm>({ resolver: zodResolver(inviteSchema), defaultValues: { role: 'Marketing' } })

  const onSubmit = (data: InviteForm) => {
    invite.mutate(
      { email: data.email, fullName: data.fullName, role: data.role as PlatformRole, password: data.password },
      {
        onSuccess: () => {
          toast.success('Kullanıcı eklendi.')
          setInviteOpen(false)
          reset()
        },
        onError: () => toast.error('E-posta zaten kayıtlı veya hata oluştu.'),
      }
    )
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Ekip Yönetimi</h1>
          <p className="text-sm text-muted-foreground">Platform çalışanlarını yönetin.</p>
        </div>
        {isSuperAdmin && (
          <Button onClick={() => setInviteOpen(true)}>
            <UserPlus className="mr-2 h-4 w-4" />
            Yeni Üye Ekle
          </Button>
        )}
      </div>

      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center gap-2 text-base">
            <Users className="h-4 w-4" />
            Platform Kullanıcıları
          </CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-3">
              {[...Array(3)].map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-muted-foreground">
                    <th className="pb-3 pr-4 font-medium">İsim</th>
                    <th className="pb-3 pr-4 font-medium">E-posta</th>
                    <th className="pb-3 pr-4 font-medium">Rol</th>
                    <th className="pb-3 pr-4 font-medium">Durum</th>
                    <th className="pb-3 pr-4 font-medium">Son Giriş</th>
                    {isSuperAdmin && <th className="pb-3 font-medium">İşlemler</th>}
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {users?.map((user) => (
                    <tr key={user.id} className="py-3">
                      <td className="py-3 pr-4 font-medium">{user.fullName}</td>
                      <td className="py-3 pr-4 text-muted-foreground">{user.email}</td>
                      <td className="py-3 pr-4">
                        {isSuperAdmin ? (
                          <RoleSelect user={user} />
                        ) : (
                          <Badge variant={ROLE_VARIANTS[user.role]}>{ROLE_LABELS[user.role]}</Badge>
                        )}
                      </td>
                      <td className="py-3 pr-4">
                        <Badge variant={user.isActive ? 'default' : 'secondary'}>
                          {user.isActive ? 'Aktif' : 'Pasif'}
                        </Badge>
                      </td>
                      <td className="py-3 pr-4 text-muted-foreground">
                        {user.lastLoginAt
                          ? format(new Date(user.lastLoginAt), 'd MMM yyyy HH:mm', { locale: tr })
                          : '—'}
                      </td>
                      {isSuperAdmin && (
                        <td className="py-3">
                          <ToggleActiveButton user={user} currentUserEmail={currentUser?.email} />
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Yeni Üye Modal */}
      <Dialog open={inviteOpen} onOpenChange={setInviteOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Yeni Üye Ekle</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="fullName">Tam İsim</Label>
              <Input id="fullName" {...register('fullName')} placeholder="Ali Veli" />
              {errors.fullName && <p className="text-xs text-destructive">{errors.fullName.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="email">E-posta</Label>
              <Input id="email" type="email" {...register('email')} placeholder="ali@tablewise.com.tr" />
              {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label>Rol</Label>
              <Select defaultValue="Marketing" onValueChange={(v) => setValue('role', v as PlatformRole)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="SuperAdmin">Süper Admin</SelectItem>
                  <SelectItem value="Marketing">Pazarlama</SelectItem>
                  <SelectItem value="Finance">Finans</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="password">Şifre</Label>
              <Input id="password" type="password" {...register('password')} />
              {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="passwordConfirm">Şifre Tekrar</Label>
              <Input id="passwordConfirm" type="password" {...register('passwordConfirm')} />
              {errors.passwordConfirm && (
                <p className="text-xs text-destructive">{errors.passwordConfirm.message}</p>
              )}
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => { setInviteOpen(false); reset() }}>
                İptal
              </Button>
              <Button type="submit" disabled={invite.isPending}>
                {invite.isPending ? 'Ekleniyor...' : 'Ekle'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
