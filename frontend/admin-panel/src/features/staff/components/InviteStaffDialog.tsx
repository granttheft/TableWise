import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { toast } from 'sonner'

const inviteSchema = z.object({
  email: z.string().email('Geçerli bir email adresi giriniz'),
  role: z.enum(['Owner', 'Staff']),
})

type InviteForm = z.infer<typeof inviteSchema>

interface InviteStaffDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function InviteStaffDialog({ open, onOpenChange }: InviteStaffDialogProps) {
  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<InviteForm>({
    resolver: zodResolver(inviteSchema),
    defaultValues: {
      email: '',
      role: 'Staff',
    },
  })

  const onSubmit = async (data: InviteForm) => {
    try {
      console.log('Sending invite:', data)
      // TODO: POST /api/v1/staff/invite
      toast.success('Davet gönderildi')
      reset()
      onOpenChange(false)
    } catch (error) {
      toast.error('Davet gönderilemedi')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Yeni Ekip Üyesi Davet Et</DialogTitle>
          <DialogDescription>
            Email adresine davet linki gönderilecektir
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">Email Adresi *</Label>
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
            <Label>Rol *</Label>
            <Controller
              name="role"
              control={control}
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Staff">Staff</SelectItem>
                    <SelectItem value="Owner">Owner</SelectItem>
                  </SelectContent>
                </Select>
              )}
            />
            <p className="text-sm text-muted-foreground">
              Owner: Tüm yetkilere sahip. Staff: Sınırlı yetkiler (plan ayarları, billing göremez)
            </p>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              İptal
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Gönderiliyor...' : 'Davet Gönder'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
