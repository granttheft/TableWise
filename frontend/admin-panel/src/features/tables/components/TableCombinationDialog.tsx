import { useEffect, useState } from 'react'
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
import { Checkbox } from '@/components/ui/checkbox'
import type { Table } from '@/types/api'

const combinationFormSchema = z.object({
  name: z.string().min(1, 'Birleşim adı zorunludur').max(100, 'Birleşim adı en fazla 100 karakter olabilir'),
  tableIds: z.array(z.string()).min(2, 'En az 2 masa seçmelisiniz'),
  combinedCapacity: z.number().min(1, 'Kapasite en az 1 olmalıdır'),
})

type CombinationFormData = z.infer<typeof combinationFormSchema>

interface TableCombinationDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  tables: Table[]
  onSubmit: (data: CombinationFormData) => void
  isLoading?: boolean
}

export function TableCombinationDialog({
  open,
  onOpenChange,
  tables,
  onSubmit,
  isLoading,
}: TableCombinationDialogProps) {
  const [selectedTableIds, setSelectedTableIds] = useState<string[]>([])

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch,
  } = useForm<CombinationFormData>({
    resolver: zodResolver(combinationFormSchema),
    defaultValues: {
      name: '',
      tableIds: [],
      combinedCapacity: 0,
    },
  })

  const capacityValue = watch('combinedCapacity')

  useEffect(() => {
    const totalCapacity = tables
      .filter((t) => selectedTableIds.includes(t.id))
      .reduce((sum, t) => sum + t.capacity, 0)
    
    setValue('combinedCapacity', totalCapacity)
    setValue('tableIds', selectedTableIds)
  }, [selectedTableIds, tables, setValue])

  useEffect(() => {
    if (!open) {
      setSelectedTableIds([])
      reset()
    }
  }, [open, reset])

  const handleTableToggle = (tableId: string) => {
    setSelectedTableIds((prev) =>
      prev.includes(tableId)
        ? prev.filter((id) => id !== tableId)
        : [...prev, tableId]
    )
  }

  const handleFormSubmit = (data: CombinationFormData) => {
    onSubmit(data)
    setSelectedTableIds([])
    reset()
  }

  const activeTables = tables.filter((t) => t.isActive)

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Yeni Masa Birleşimi</DialogTitle>
          <DialogDescription>
            Birden fazla masayı birleştirerek daha büyük bir masa oluşturun.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Birleşim Adı *</Label>
            <Input
              id="name"
              placeholder="VIP Toplantı Masası"
              {...register('name')}
            />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label>Masalar * (En az 2 masa seçin)</Label>
            <div className="max-h-48 space-y-2 overflow-y-auto rounded-md border p-3">
              {activeTables.length === 0 ? (
                <p className="text-sm text-muted-foreground">Aktif masa bulunamadı</p>
              ) : (
                activeTables.map((table) => (
                  <div key={table.id} className="flex items-center space-x-2">
                    <Checkbox
                      id={`table-${table.id}`}
                      checked={selectedTableIds.includes(table.id)}
                      onCheckedChange={() => handleTableToggle(table.id)}
                    />
                    <label
                      htmlFor={`table-${table.id}`}
                      className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                    >
                      {table.name} ({table.capacity} kişi)
                    </label>
                  </div>
                ))
              )}
            </div>
            {errors.tableIds && (
              <p className="text-sm text-destructive">{errors.tableIds.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="combinedCapacity">Birleşik Kapasite *</Label>
            <Input
              id="combinedCapacity"
              type="number"
              min={1}
              {...register('combinedCapacity', { valueAsNumber: true })}
            />
            <p className="text-xs text-muted-foreground">
              Otomatik hesaplanır, ancak düzenleyebilirsiniz.
            </p>
            {errors.combinedCapacity && (
              <p className="text-sm text-destructive">{errors.combinedCapacity.message}</p>
            )}
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
            <Button type="submit" disabled={isLoading || activeTables.length < 2}>
              {isLoading ? 'Oluşturuluyor...' : 'Oluştur'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
