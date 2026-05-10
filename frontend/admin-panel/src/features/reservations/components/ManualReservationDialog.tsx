import { useState, useEffect } from 'react'
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
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Checkbox } from '@/components/ui/checkbox'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { AlertCircle, Search, UserPlus, CheckCircle, XCircle } from 'lucide-react'
import { toast } from 'sonner'
import { useSearchCustomers, useCreateReservation, useEvaluateReservation } from '@/hooks/useReservations'
import type { Table, VenueCustomField, Customer } from '@/types/api'

const manualReservationSchema = z.object({
  venueId: z.string().uuid(),
  tableId: z.string().uuid(),
  date: z.string(),
  timeSlot: z.string(),
  guestCount: z.number().min(1).max(50),
  customerId: z.string().uuid().optional(),
  guestName: z.string().min(1, 'Misafir adı zorunludur'),
  guestEmail: z.string().email('Geçerli email giriniz').optional().or(z.literal('')),
  guestPhone: z.string().optional(),
  specialRequest: z.string().optional(),
  customFields: z.record(z.any()).optional(),
  overrideRules: z.boolean().default(false),
})

type ManualReservationForm = z.infer<typeof manualReservationSchema>

interface ManualReservationDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  venueId: string
  tables: Table[]
  customFields?: VenueCustomField[]
  presetTableId?: string
  presetTimeSlot?: string
  presetDate?: string
}

export function ManualReservationDialog({
  open,
  onOpenChange,
  venueId,
  tables,
  customFields = [],
  presetTableId,
  presetTimeSlot,
  presetDate,
}: ManualReservationDialogProps) {
  const [customerSearchTerm, setCustomerSearchTerm] = useState('')
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null)
  const [showNewCustomerForm, setShowNewCustomerForm] = useState(false)
  const [evaluationResult, setEvaluationResult] = useState<any>(null)

  const { data: searchResults, isLoading: isSearching } = useSearchCustomers(
    customerSearchTerm,
    { enabled: customerSearchTerm.length >= 2 }
  )

  const createReservation = useCreateReservation()
  const evaluateReservation = useEvaluateReservation()

  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ManualReservationForm>({
    resolver: zodResolver(manualReservationSchema),
    defaultValues: {
      venueId,
      tableId: presetTableId || '',
      date: presetDate || new Date().toISOString().split('T')[0],
      timeSlot: presetTimeSlot || '',
      guestCount: 2,
      guestName: '',
      guestEmail: '',
      guestPhone: '',
      specialRequest: '',
      customFields: {},
      overrideRules: false,
    },
  })

  const overrideRules = watch('overrideRules')

  useEffect(() => {
    if (open) {
      reset({
        venueId,
        tableId: presetTableId || '',
        date: presetDate || new Date().toISOString().split('T')[0],
        timeSlot: presetTimeSlot || '',
        guestCount: 2,
        guestName: '',
        guestEmail: '',
        guestPhone: '',
        specialRequest: '',
        customFields: {},
        overrideRules: false,
      })
      setSelectedCustomer(null)
      setShowNewCustomerForm(false)
      setEvaluationResult(null)
    }
  }, [open, reset, venueId, presetTableId, presetDate, presetTimeSlot])

  const handleCustomerSelect = (customer: Customer) => {
    setSelectedCustomer(customer)
    setValue('customerId', customer.id)
    setValue('guestName', customer.fullName)
    setValue('guestEmail', customer.email || '')
    setValue('guestPhone', customer.phone || '')
    setCustomerSearchTerm('')
  }

  const handleCheckRules = async () => {
    const values = watch()
    try {
      const result = await evaluateReservation.mutateAsync({
        venueId: values.venueId,
        tableId: values.tableId,
        date: values.date,
        timeSlot: values.timeSlot,
        guestCount: values.guestCount,
        customerId: values.customerId,
      })
      setEvaluationResult(result)
    } catch (error) {
      toast.error('Kurallar kontrol edilemedi')
    }
  }

  const onSubmit = async (data: ManualReservationForm) => {
    try {
      await createReservation.mutateAsync(data)
      toast.success('Rezervasyon oluşturuldu')
      onOpenChange(false)
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Rezervasyon oluşturulamadı')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Manuel Rezervasyon</DialogTitle>
          <DialogDescription>Yeni rezervasyon oluştur</DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Masa & Tarih & Saat */}
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Masa *</Label>
              <Controller
                name="tableId"
                control={control}
                render={({ field }) => (
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger>
                      <SelectValue placeholder="Masa seçin" />
                    </SelectTrigger>
                    <SelectContent>
                      {tables.map((table) => (
                        <SelectItem key={table.id} value={table.id}>
                          {table.name} ({table.capacity} kişi)
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
              {errors.tableId && <p className="text-sm text-destructive">{errors.tableId.message}</p>}
            </div>

            <div className="space-y-2">
              <Label>Kişi Sayısı *</Label>
              <Input type="number" min={1} max={50} {...register('guestCount', { valueAsNumber: true })} />
              {errors.guestCount && <p className="text-sm text-destructive">{errors.guestCount.message}</p>}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Tarih *</Label>
              <Input type="date" {...register('date')} />
              {errors.date && <p className="text-sm text-destructive">{errors.date.message}</p>}
            </div>

            <div className="space-y-2">
              <Label>Saat *</Label>
              <Input type="time" {...register('timeSlot')} />
              {errors.timeSlot && <p className="text-sm text-destructive">{errors.timeSlot.message}</p>}
            </div>
          </div>

          {/* Müşteri Arama */}
          <div className="space-y-2">
            <Label>Müşteri</Label>
            {!selectedCustomer ? (
              <>
                <div className="flex gap-2">
                  <div className="relative flex-1">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input
                      placeholder="Email veya telefon ile ara..."
                      value={customerSearchTerm}
                      onChange={(e) => setCustomerSearchTerm(e.target.value)}
                      className="pl-9"
                    />
                  </div>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setShowNewCustomerForm(!showNewCustomerForm)}
                  >
                    <UserPlus className="mr-2 h-4 w-4" />
                    Yeni Müşteri
                  </Button>
                </div>

                {isSearching && <p className="text-sm text-muted-foreground">Aranıyor...</p>}

                {searchResults && searchResults.length > 0 && (
                  <div className="rounded-lg border divide-y max-h-40 overflow-y-auto">
                    {searchResults.map((customer) => (
                      <button
                        key={customer.id}
                        type="button"
                        onClick={() => handleCustomerSelect(customer)}
                        className="w-full px-3 py-2 text-left hover:bg-muted transition-colors"
                      >
                        <div className="font-medium">{customer.fullName}</div>
                        <div className="text-sm text-muted-foreground">
                          {customer.email} • {customer.phone}
                        </div>
                      </button>
                    ))}
                  </div>
                )}
              </>
            ) : (
              <div className="flex items-center justify-between rounded-lg border p-3">
                <div>
                  <div className="font-medium">{selectedCustomer.fullName}</div>
                  <div className="text-sm text-muted-foreground">
                    {selectedCustomer.email} • {selectedCustomer.phone}
                  </div>
                </div>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setSelectedCustomer(null)
                    setValue('customerId', undefined)
                  }}
                >
                  Değiştir
                </Button>
              </div>
            )}
          </div>

          {/* Misafir Bilgileri */}
          <div className="space-y-4 rounded-lg border p-4">
            <h4 className="font-medium">Misafir Bilgileri</h4>

            <div className="space-y-2">
              <Label>İsim *</Label>
              <Input {...register('guestName')} disabled={!!selectedCustomer} />
              {errors.guestName && <p className="text-sm text-destructive">{errors.guestName.message}</p>}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Email</Label>
                <Input type="email" {...register('guestEmail')} disabled={!!selectedCustomer} />
                {errors.guestEmail && <p className="text-sm text-destructive">{errors.guestEmail.message}</p>}
              </div>

              <div className="space-y-2">
                <Label>Telefon</Label>
                <Input {...register('guestPhone')} disabled={!!selectedCustomer} />
              </div>
            </div>

            <div className="space-y-2">
              <Label>Özel İstek</Label>
              <Textarea {...register('specialRequest')} rows={2} placeholder="Pencere kenarı, doğum günü vb." />
            </div>
          </div>

          {/* Custom Fields */}
          {customFields.length > 0 && (
            <div className="space-y-4 rounded-lg border p-4">
              <h4 className="font-medium">Ek Bilgiler</h4>
              {customFields.map((field) => (
                <div key={field.id} className="space-y-2">
                  <Label>
                    {field.label}
                    {field.isRequired && ' *'}
                  </Label>
                  {field.type === 'text' && (
                    <Input {...register(`customFields.${field.id}`)} placeholder={field.placeholder} />
                  )}
                  {field.type === 'textarea' && (
                    <Textarea {...register(`customFields.${field.id}`)} placeholder={field.placeholder} rows={2} />
                  )}
                  {field.type === 'checkbox' && (
                    <Checkbox {...register(`customFields.${field.id}`)} />
                  )}
                </div>
              ))}
            </div>
          )}

          {/* Kural Kontrolü */}
          <div className="space-y-2">
            <Button type="button" variant="outline" onClick={handleCheckRules} className="w-full">
              Kuralları Kontrol Et
            </Button>

            {evaluationResult && (
              <div className="space-y-2">
                {evaluationResult.warnings?.map((warning: any, i: number) => (
                  <Alert key={i} variant="default">
                    <AlertCircle className="h-4 w-4" />
                    <AlertDescription>{warning.message}</AlertDescription>
                  </Alert>
                ))}

                {evaluationResult.blockers?.map((blocker: any, i: number) => (
                  <Alert key={i} variant="destructive">
                    <XCircle className="h-4 w-4" />
                    <AlertDescription>{blocker.message}</AlertDescription>
                  </Alert>
                ))}

                {evaluationResult.isAllowed && (
                  <Alert>
                    <CheckCircle className="h-4 w-4" />
                    <AlertDescription>Tüm kurallar geçildi, rezervasyon oluşturulabilir</AlertDescription>
                  </Alert>
                )}
              </div>
            )}

            {evaluationResult && !evaluationResult.isAllowed && (
              <div className="flex items-center space-x-2">
                <Checkbox id="override" {...register('overrideRules')} />
                <label htmlFor="override" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
                  Kuralları atla (Sadece Owner)
                </label>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              İptal
            </Button>
            <Button type="submit" disabled={isSubmitting || (evaluationResult && !evaluationResult.isAllowed && !overrideRules)}>
              {isSubmitting ? 'Oluşturuluyor...' : 'Rezervasyon Oluştur'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
