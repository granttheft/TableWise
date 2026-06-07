import { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, ChevronLeft, ChevronRight } from 'lucide-react'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { useTenants } from '@/hooks/useTenants'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import type { PlanStatus } from '@/types/api'

const PAGE_SIZE = 15

const statusOptions: { label: string; value: PlanStatus | '' }[] = [
  { label: 'Tümü', value: '' },
  { label: 'Aktif', value: 'Active' },
  { label: 'Trial', value: 'TrialActive' },
  { label: 'Askıya Alınan', value: 'Suspended' },
  { label: 'İptal Edildi', value: 'Cancelled' },
]

const statusBadge: Record<PlanStatus, { label: string; class: string }> = {
  Active: { label: 'Aktif', class: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' },
  TrialActive: { label: 'Trial', class: 'bg-blue-500/20 text-blue-300 border-blue-500/30' },
  TrialExpired: { label: 'Trial Bitti', class: 'bg-orange-500/20 text-orange-300 border-orange-500/30' },
  Suspended: { label: 'Askıda', class: 'bg-red-500/20 text-red-300 border-red-500/30' },
  Cancelled: { label: 'İptal', class: 'bg-zinc-500/20 text-zinc-300 border-zinc-500/30' },
}

export function TenantsPage() {
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [status, setStatus] = useState<PlanStatus | ''>('')
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    if (timerRef.current) clearTimeout(timerRef.current)
    timerRef.current = setTimeout(() => {
      setDebouncedSearch(search)
      setPage(1)
    }, 300)
    return () => { if (timerRef.current) clearTimeout(timerRef.current) }
  }, [search])

  const { data, isLoading } = useTenants({
    page,
    pageSize: PAGE_SIZE,
    search: debouncedSearch,
    status,
  })

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-bold">Müşteriler</h1>
        <p className="text-sm text-muted-foreground">Tüm tenant'ları görüntüle ve yönet</p>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Ad veya e-posta ara..."
            className="pl-9"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <Select value={status} onValueChange={(v) => { setStatus(v as PlanStatus | ''); setPage(1) }}>
          <SelectTrigger className="w-44">
            <SelectValue placeholder="Durum" />
          </SelectTrigger>
          <SelectContent>
            {statusOptions.map((opt) => (
              <SelectItem key={opt.value} value={opt.value === '' ? '__all__' : opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <div className="rounded-lg border border-border overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border bg-muted/50">
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">Restoran</th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">Plan</th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">Durum</th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">Kayıt Tarihi</th>
              <th className="px-4 py-3 text-right font-medium text-muted-foreground">Bu Ay Rezervasyon</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              Array.from({ length: 8 }).map((_, i) => (
                <tr key={i} className="border-b border-border last:border-0">
                  {Array.from({ length: 5 }).map((_, j) => (
                    <td key={j} className="px-4 py-3">
                      <Skeleton className="h-4 w-full" />
                    </td>
                  ))}
                </tr>
              ))
            ) : data?.items.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-muted-foreground">
                  Sonuç bulunamadı.
                </td>
              </tr>
            ) : (
              data?.items.map((tenant) => {
                const badge = statusBadge[tenant.planStatus] ?? { label: tenant.planStatus, class: 'bg-zinc-500/20 text-zinc-300 border-zinc-500/30' }
                return (
                  <tr
                    key={tenant.id}
                    className="border-b border-border last:border-0 cursor-pointer hover:bg-muted/40 transition-colors"
                    onClick={() => navigate(`/tenants/${tenant.id}`)}
                  >
                    <td className="px-4 py-3">
                      <div className="font-medium">{tenant.name}</div>
                      <div className="text-xs text-muted-foreground">{tenant.email}</div>
                    </td>
                    <td className="px-4 py-3 text-muted-foreground">{tenant.planName}</td>
                    <td className="px-4 py-3">
                      <span className={`rounded-full border px-2 py-0.5 text-xs font-medium ${badge.class}`}>
                        {badge.label}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {format(new Date(tenant.createdAt), 'd MMM yyyy', { locale: tr })}
                    </td>
                    <td className="px-4 py-3 text-right text-muted-foreground">
                      {tenant.reservationCountThisMonth}
                    </td>
                  </tr>
                )
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            {data.totalCount} kayıttan {(page - 1) * PAGE_SIZE + 1}–{Math.min(page * PAGE_SIZE, data.totalCount)} gösteriliyor
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
            >
              <ChevronLeft className="h-4 w-4" />
              Önceki
            </Button>
            <span className="text-sm text-muted-foreground">
              {page} / {data.totalPages}
            </span>
            <Button
              variant="outline"
              size="sm"
              disabled={page >= data.totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Sonraki
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
