import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
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
import { Textarea } from '@/components/ui/textarea'
import { Switch } from '@/components/ui/switch'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { Table, TableLocation } from '@/types/api'

const tableFormSchema = z.object({
  name: z.string().min(1, 'Masa adı zorunludur').max(50, 'Masa adı en fazla 50 karakter olabilir'),
  capacity: z.number().min(1, 'Kapasite en az 1 olmalıdır').max(50, 'Kapasite en fazla 50 olabilir'),
  location: z.enum(['Indoor', 'Outdoor', 'Balcony', 'Bar', 'Private', 'Terrace', 'Garden']),
  description: z.string().max(500, 'Açıklama en fazla 500 karakter olabilir').optional(),
  isActive: z.boolean(),
})

type TableFormData = z.infer<typeof tableFormSchema>

interface TableFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  table?: Table | null
  onSubmit: (data: TableFormData) => void
  isLoading?: boolean
}

const locationOptions: { value: TableLocation; label: string }[] = [
  { value: 'Indoor', label: 'İç Mekan' },
  { value: 'Outdoor', label: 'Dış Mekan' },
  { value: 'Balcony', label: 'Balkon' },
  { value: 'Bar', label: 'Bar' },
  { value: 'Private', label: 'Özel Salon' },
  { value: 'Terrace', label: 'Teras' },
  { value: 'Garden', label: 'Bahçe' },
]

export function TableFormDialog({
  open,
  onOpenChange,
  table,
  onSubmit,
  isLoading,
}: TableFormDialogProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch,
  } = useForm<TableFormData>({
    resolver: zodResolver(tableFormSchema),
    defaultValues: {
      name: '',
      capacity: 4,
      location: 'Indoor',
      description: '',
      isActive: true,
    },
  })

  const locationValue = watch('location')
  const isActiveValue = watch('isActive')

  useEffect(() => {
    if (table) {
      reset({
        name: table.name,
        capacity: table.capacity,
        location: table.location,
        description: table.description || '',
        isActive: table.isActive,
      })
    } else {
      reset({
        name: '',
        capacity: 4,
        location: 'Indoor',
        description: '',
        isActive: true,
      })
    }
  }, [table, reset])

  const handleFormSubmit = (data: TableFormData) => {
    onSubmit(data)
    if (!table) {
      reset()
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>{table ? 'Masa Düzenle' : 'Yeni Masa'}</DialogTitle>
          <DialogDescription>
            {table ? 'Masa bilgilerini güncelleyin.' : 'Yeni masa ekleyin.'}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Masa Adı *</Label>
            <Input
              id="name"
              placeholder="Masa 1"
              {...register('name')}
            />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="capacity">Kapasite *</Label>
            <Input
              id="capacity"
              type="number"
              min={1}
              max={50}
              {...register('capacity', { valueAsNumber: true })}
            />
            {errors.capacity && (
              <p className="text-sm text-destructive">{errors.capacity.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="location">Konum *</Label>
            <Select
              value={locationValue}
              onValueChange={(value) => setValue('location', value as TableLocation)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Konum seçin" />
              </SelectTrigger>
              <SelectContent>
                {locationOptions.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.location && (
              <p className="text-sm text-destructive">{errors.location.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Açıklama</Label>
            <Textarea
              id="description"
              placeholder="Masa hakkında ek bilgiler (opsiyonel)"
              rows={3}
              {...register('description')}
            />
            {errors.description && (
              <p className="text-sm text-destructive">{errors.description.message}</p>
            )}
          </div>

          <div className="flex items-center space-x-2">
            <Switch
              id="isActive"
              checked={isActiveValue}
              onCheckedChange={(checked) => setValue('isActive', checked)}
            />
            <Label htmlFor="isActive">Aktif</Label>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              İptal
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? 'Kaydediliyor...' : table ? 'Güncelle' : 'Oluştur'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
