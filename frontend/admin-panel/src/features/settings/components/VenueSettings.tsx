import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Store } from 'lucide-react'
import { useVenues, useCreateVenue } from '@/hooks/useVenues'

const createVenueSchema = z.object({
  name: z.string().min(2, 'En az 2 karakter').max(100, 'En fazla 100 karakter'),
  address: z.string().max(500).optional(),
})

type CreateVenueForm = z.infer<typeof createVenueSchema>

/**
 * Mekan listesi ve yeni mekan oluşturma (API ile); Masalar/Kurallar sayfaları aynı cache anahtarını kullanır.
 */
export function VenueSettings() {
  const { data: venues = [], isLoading } = useVenues()
  const createVenue = useCreateVenue()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateVenueForm>({
    resolver: zodResolver(createVenueSchema),
    defaultValues: { name: '', address: '' },
  })

  const onSubmit = (values: CreateVenueForm) => {
    createVenue.mutate(
      {
        name: values.name.trim(),
        address: values.address?.trim() || undefined,
      },
      {
        onSuccess: () => reset({ name: '', address: '' }),
      }
    )
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Store className="h-5 w-5" />
            Mekanlarım
          </CardTitle>
          <CardDescription>
            İşletme adı (Genel sekmesi) ile mekan aynı şey değildir. Masalar ve kurallar bir mekana bağlanır; önce burada
            mekan oluşturun.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <p className="text-sm text-muted-foreground">Yükleniyor…</p>
          ) : venues.length === 0 ? (
            <p className="text-sm text-muted-foreground">Henüz kayıtlı mekan yok. Aşağıdan ilk mekanınızı ekleyin.</p>
          ) : (
            <ul className="space-y-2">
              {venues.map((v) => (
                <li
                  key={v.id}
                  className="flex items-center justify-between rounded-md border px-3 py-2 text-sm"
                >
                  <span className="font-medium">{v.name}</span>
                  <span className="text-muted-foreground">{v.tableCount ?? 0} masa</span>
                </li>
              ))}
            </ul>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Yeni mekan</CardTitle>
          <CardDescription>Owner hesabı ile yeni mekan oluşturabilirsiniz (plan limitine tabidir).</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 max-w-md">
            <div className="space-y-2">
              <Label htmlFor="venue-name">Mekan adı *</Label>
              <Input id="venue-name" placeholder="Örn: Ana Şube" {...register('name')} disabled={createVenue.isPending} />
              {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="venue-address">Adres (isteğe bağlı)</Label>
              <Input id="venue-address" {...register('address')} disabled={createVenue.isPending} />
              {errors.address && <p className="text-sm text-destructive">{errors.address.message}</p>}
            </div>
            <Button type="submit" disabled={createVenue.isPending}>
              {createVenue.isPending ? 'Oluşturuluyor…' : 'Mekan oluştur'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
