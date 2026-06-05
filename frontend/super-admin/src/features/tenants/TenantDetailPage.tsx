import { useParams, useNavigate } from 'react-router-dom'
import { ArrowLeft, Building2, Users, MapPin, Calendar, FileText, MessageSquare } from 'lucide-react'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { useTenantDetail } from '@/hooks/useTenantDetail'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import type { PlanStatus } from '@/types/api'

const statusBadge: Record<PlanStatus, { label: string; class: string }> = {
  Active: { label: 'Aktif', class: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' },
  TrialActive: { label: 'Trial', class: 'bg-blue-500/20 text-blue-300 border-blue-500/30' },
  TrialExpired: { label: 'Trial Bitti', class: 'bg-orange-500/20 text-orange-300 border-orange-500/30' },
  Suspended: { label: 'Askıda', class: 'bg-red-500/20 text-red-300 border-red-500/30' },
  Cancelled: { label: 'İptal', class: 'bg-zinc-500/20 text-zinc-300 border-zinc-500/30' },
}

function InfoRow({ icon: Icon, label, value }: { icon: React.ComponentType<{ className?: string }>; label: string; value?: string | number | null }) {
  if (!value) return null
  return (
    <div className="flex items-start gap-3 py-2">
      <Icon className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" />
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="text-sm font-medium">{value}</p>
      </div>
    </div>
  )
}

function fmt(dateStr?: string | null) {
  if (!dateStr) return null
  return format(new Date(dateStr), 'd MMM yyyy', { locale: tr })
}

export function TenantDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: tenant, isLoading } = useTenantDetail(id ?? '')

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  if (!tenant) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/tenants')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Geri
        </Button>
        <p className="text-muted-foreground">Tenant bulunamadı.</p>
      </div>
    )
  }

  const badge = statusBadge[tenant.planStatus]

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/tenants')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Geri
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold">{tenant.name}</h1>
            <span className={`rounded-full border px-2 py-0.5 text-xs font-medium ${badge.class}`}>
              {badge.label}
            </span>
          </div>
          <p className="text-sm text-muted-foreground">{tenant.email}</p>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="general">
        <TabsList>
          <TabsTrigger value="general">Genel</TabsTrigger>
          <TabsTrigger value="subscription">Abonelik</TabsTrigger>
          <TabsTrigger value="notes">Notlar</TabsTrigger>
        </TabsList>

        {/* Genel */}
        <TabsContent value="general">
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Tenant Bilgileri</CardTitle>
              </CardHeader>
              <CardContent className="divide-y divide-border">
                <InfoRow icon={MapPin} label="Slug" value={tenant.slug} />
                <InfoRow icon={Calendar} label="Kayıt Tarihi" value={fmt(tenant.createdAt)} />
                <InfoRow icon={Calendar} label="Trial Bitiş" value={fmt(tenant.trialEndsAt)} />
                <InfoRow icon={Calendar} label="Plan Yenileme" value={fmt(tenant.planRenewsAt)} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="text-base">Kullanım</CardTitle>
              </CardHeader>
              <CardContent className="divide-y divide-border">
                <InfoRow icon={Building2} label="Mekan Sayısı" value={tenant.venueCount} />
                <InfoRow icon={Users} label="Kullanıcı Sayısı" value={tenant.userCount} />
                <InfoRow icon={FileText} label="Bu Ay Rezervasyon" value={tenant.reservationCountThisMonth} />
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Abonelik */}
        <TabsContent value="subscription">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Abonelik Bilgileri</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="divide-y divide-border">
                <InfoRow icon={FileText} label="Plan" value={tenant.planName} />
                {tenant.subscriptionAmount && (
                  <InfoRow
                    icon={FileText}
                    label="Aylık Tutar"
                    value={new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(tenant.subscriptionAmount)}
                  />
                )}
                <InfoRow icon={Calendar} label="Dönem Başlangıç" value={fmt(tenant.subscriptionPeriodStart)} />
                <InfoRow icon={Calendar} label="Dönem Bitiş" value={fmt(tenant.subscriptionPeriodEnd)} />
              </div>
              <div className="mt-6 rounded-lg border border-border bg-muted/30 p-4">
                <p className="text-sm text-muted-foreground">
                  💡 Ödeme detayları ve fatura geçmişi <strong>Faz 7 (İyzico)</strong> tamamlandıktan sonra bu bölümde aktif olacak.
                </p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Notlar */}
        <TabsContent value="notes">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Platform Notları</CardTitle>
            </CardHeader>
            <CardContent>
              {tenant.notes.length === 0 ? (
                <div className="py-8 text-center text-muted-foreground">
                  <MessageSquare className="mx-auto mb-2 h-8 w-8 opacity-40" />
                  <p className="text-sm">Henüz not eklenmemiş.</p>
                  <p className="text-xs mt-1 opacity-60">Not ekleme özelliği Faz 8.5.3'te gelecek.</p>
                </div>
              ) : (
                <div className="space-y-3">
                  {tenant.notes.map((note) => (
                    <div key={note.id} className="rounded-lg border border-border p-4">
                      <p className="text-sm">{note.content}</p>
                      <div className="mt-2 flex items-center gap-2 text-xs text-muted-foreground">
                        <span>{note.createdByEmail}</span>
                        <span>·</span>
                        <span>{fmt(note.createdAt)}</span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}
