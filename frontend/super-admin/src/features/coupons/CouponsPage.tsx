import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Search, Loader2, Tag, XCircle } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { toast } from 'sonner'
import api from '@/lib/api'
import { useAuth } from '@/hooks/useAuth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  Dialog, DialogContent, DialogHeader,
  DialogTitle, DialogFooter,
} from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'

interface CouponDto {
  id: string
  code: string
  discountType: number  // 0=Percentage, 1=FixedAmount
  discountValue: number
  usageLimit: number | null
  usedCount: number
  expiresAt: string | null
  isActive: boolean
  applicablePlans: string | null
  createdByEmail: string
  createdAt: string
}

interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

const couponSchema = z.object({
  code: z.string().min(3, 'Minimum 3 karakter').max(20),
  discountType: z.enum(['0', '1']),
  discountValue: z.coerce.number().min(0.01),
  usageLimit: z.coerce.number().int().min(1).optional().or(z.literal('')),
  expiresAt: z.string().optional(),
  applicablePlans: z.string().optional(),
})

type CouponForm = z.infer<typeof couponSchema>

const DISCOUNT_LABEL = (type: number, value: number) =>
  type === 0 ? `%${value}` : `₺${value.toFixed(2)}`

function fmt(d?: string | null) {
  if (!d) return null
  return format(new Date(d), 'd MMM yyyy', { locale: tr })
}

export function CouponsPage() {
  const queryClient = useQueryClient()
  const { isSuperAdmin, isMarketing } = useAuth()
  const canCreate = isSuperAdmin || isMarketing

  const [search, setSearch] = useState('')
  const [activeFilter, setActiveFilter] = useState<'all' | 'true' | 'false'>('all')
  const [page, setPage] = useState(1)
  const [modalOpen, setModalOpen] = useState(false)

  const params = {
    page,
    pageSize: 20,
    ...(search ? { search } : {}),
    ...(activeFilter !== 'all' ? { isActive: activeFilter } : {}),
  }

  const { data, isLoading } = useQuery<PagedResult<CouponDto>>({
    queryKey: ['platform-coupons', params],
    queryFn: () => api.get('/api/platform/coupons', { params }).then((r) => r.data),
    staleTime: 30_000,
  })

  const deactivateMutation = useMutation({
    mutationFn: (id: string) => api.put(`/api/platform/coupons/${id}/deactivate`),
    onSuccess: () => {
      toast.success('Kupon deaktif edildi.')
      queryClient.invalidateQueries({ queryKey: ['platform-coupons'] })
    },
    onError: () => toast.error('İşlem başarısız.'),
  })

  const createMutation = useMutation({
    mutationFn: (dto: object) => api.post('/api/platform/coupons', dto),
    onSuccess: () => {
      toast.success('Kupon oluşturuldu.')
      queryClient.invalidateQueries({ queryKey: ['platform-coupons'] })
      setModalOpen(false)
      reset()
    },
    onError: () => toast.error('Kupon oluşturulamadı.'),
  })

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CouponForm>({ resolver: zodResolver(couponSchema) })

  function onSubmit(data: CouponForm) {
    createMutation.mutate({
      code: data.code.toUpperCase(),
      discountType: parseInt(data.discountType),
      discountValue: data.discountValue,
      usageLimit: data.usageLimit ? Number(data.usageLimit) : null,
      expiresAt: data.expiresAt || null,
      applicablePlans: data.applicablePlans || null,
    })
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Kuponlar</h1>
          <p className="text-sm text-muted-foreground mt-1">Platform kupon ve indirim kodlarını yönetin.</p>
        </div>
        {canCreate && (
          <Button onClick={() => { setModalOpen(true); reset() }}>
            <Plus className="mr-2 h-4 w-4" />
            Yeni Kupon
          </Button>
        )}
      </div>

      {/* Filtreler */}
      <div className="flex gap-3">
        <div className="relative flex-1 max-w-xs">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Kupon kodu ara..."
            className="pl-9"
            value={search}
            onChange={(e) => { setSearch(e.target.value.toUpperCase()); setPage(1) }}
          />
        </div>
        <Select value={activeFilter} onValueChange={(v) => { setActiveFilter(v as typeof activeFilter); setPage(1) }}>
          <SelectTrigger className="w-36">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Tümü</SelectItem>
            <SelectItem value="true">Aktif</SelectItem>
            <SelectItem value="false">Pasif</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Tablo */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">
            {data ? `${data.totalCount} kupon` : 'Kuponlar'}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-2">
              {[...Array(5)].map((_, i) => <Skeleton key={i} className="h-12" />)}
            </div>
          ) : !data?.items.length ? (
            <div className="py-12 text-center text-muted-foreground">
              <Tag className="mx-auto mb-2 h-8 w-8 opacity-40" />
              <p className="text-sm">Kupon bulunamadı.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-xs text-muted-foreground">
                    <th className="pb-3 pr-4 font-medium">Kod</th>
                    <th className="pb-3 pr-4 font-medium">İndirim</th>
                    <th className="pb-3 pr-4 font-medium">Kullanım</th>
                    <th className="pb-3 pr-4 font-medium">Geçerlilik</th>
                    <th className="pb-3 pr-4 font-medium">Durum</th>
                    {canCreate && <th className="pb-3 font-medium"></th>}
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {data.items.map((coupon) => (
                    <tr key={coupon.id} className="hover:bg-muted/30 transition-colors">
                      <td className="py-3 pr-4 font-mono font-medium">{coupon.code}</td>
                      <td className="py-3 pr-4">
                        {DISCOUNT_LABEL(coupon.discountType, coupon.discountValue)}
                      </td>
                      <td className="py-3 pr-4">
                        {coupon.usedCount} / {coupon.usageLimit ?? '∞'}
                      </td>
                      <td className="py-3 pr-4 text-muted-foreground">
                        {fmt(coupon.expiresAt) ?? 'Süresiz'}
                      </td>
                      <td className="py-3 pr-4">
                        <span className={`inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium ${
                          coupon.isActive
                            ? 'border-emerald-500/30 bg-emerald-500/10 text-emerald-400'
                            : 'border-zinc-500/30 bg-zinc-500/10 text-zinc-400'
                        }`}>
                          {coupon.isActive ? 'Aktif' : 'Pasif'}
                        </span>
                      </td>
                      {canCreate && (
                        <td className="py-3">
                          {coupon.isActive && (
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-7 text-muted-foreground hover:text-destructive"
                              onClick={() => deactivateMutation.mutate(coupon.id)}
                              disabled={deactivateMutation.isPending}
                            >
                              <XCircle className="h-4 w-4" />
                            </Button>
                          )}
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Sayfalama */}
          {data && data.totalPages > 1 && (
            <div className="mt-4 flex items-center justify-between text-sm text-muted-foreground">
              <span>{data.totalCount} sonuçtan {(page - 1) * 20 + 1}–{Math.min(page * 20, data.totalCount)}</span>
              <div className="flex gap-2">
                <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                  Önceki
                </Button>
                <Button variant="outline" size="sm" disabled={page >= data.totalPages} onClick={() => setPage((p) => p + 1)}>
                  Sonraki
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Yeni Kupon Modal */}
      <Dialog open={modalOpen} onOpenChange={setModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Yeni Kupon Oluştur</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="code">Kupon Kodu</Label>
              <Input
                id="code"
                placeholder="SUMMER20"
                {...register('code')}
                onChange={(e) => {
                  e.target.value = e.target.value.toUpperCase()
                  register('code').onChange(e)
                }}
              />
              {errors.code && <p className="text-xs text-destructive">{errors.code.message}</p>}
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label>İndirim Tipi</Label>
                <select
                  {...register('discountType')}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
                >
                  <option value="0">Yüzde (%)</option>
                  <option value="1">Sabit Tutar (₺)</option>
                </select>
              </div>
              <div className="space-y-1">
                <Label htmlFor="discountValue">İndirim Değeri</Label>
                <Input
                  id="discountValue"
                  type="number"
                  step="0.01"
                  min="0.01"
                  placeholder="10"
                  {...register('discountValue')}
                />
                {errors.discountValue && <p className="text-xs text-destructive">{errors.discountValue.message}</p>}
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label htmlFor="usageLimit">Kullanım Limiti</Label>
                <Input
                  id="usageLimit"
                  type="number"
                  min="1"
                  placeholder="Sınırsız"
                  {...register('usageLimit')}
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="expiresAt">Geçerlilik Tarihi</Label>
                <Input
                  id="expiresAt"
                  type="date"
                  {...register('expiresAt')}
                />
              </div>
            </div>

            <div className="space-y-1">
              <Label htmlFor="applicablePlans">Geçerli Planlar</Label>
              <Input
                id="applicablePlans"
                placeholder='["Starter","Pro"] veya boş bırakın (tümü)'
                {...register('applicablePlans')}
              />
              <p className="text-xs text-muted-foreground">Boş bırakırsanız tüm planlara uygulanır.</p>
            </div>

            <DialogFooter>
              <Button type="button" variant="ghost" onClick={() => setModalOpen(false)}>
                İptal
              </Button>
              <Button type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Oluştur
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
