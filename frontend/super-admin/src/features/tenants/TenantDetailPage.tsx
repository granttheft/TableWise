import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, Building2, Users, MapPin, Calendar,
  FileText, MessageSquare, AlertTriangle, Loader2,
} from 'lucide-react'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { toast } from 'sonner'
import { useTenantDetail } from '@/hooks/useTenantDetail'
import { useAuth } from '@/hooks/useAuth'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import {
  Dialog, DialogContent, DialogDescription,
  DialogFooter, DialogHeader, DialogTitle,
} from '@/components/ui/dialog'
import api from '@/lib/api'
import type { PlanStatus } from '@/types/api'

const statusBadge: Record<PlanStatus, { label: string; class: string }> = {
  Active: { label: 'Aktif', class: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' },
  TrialActive: { label: 'Trial', class: 'bg-blue-500/20 text-blue-300 border-blue-500/30' },
  TrialExpired: { label: 'Trial Bitti', class: 'bg-orange-500/20 text-orange-300 border-orange-500/30' },
  Suspended: { label: 'Askıda', class: 'bg-red-500/20 text-red-300 border-red-500/30' },
  Cancelled: { label: 'İptal', class: 'bg-zinc-500/20 text-zinc-300 border-zinc-500/30' },
}

function InfoRow({
  icon: Icon, label, value,
}: {
  icon: React.ComponentType<{ className?: string }>
  label: string
  value?: string | number | null
}) {
  if (value === undefined || value === null || value === '') return null
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

const PLAN_OPTIONS = [
  { label: 'Starter', value: 'starter' },
  { label: 'Pro', value: 'pro' },
  { label: 'Business', value: 'business' },
  { label: 'Enterprise', value: 'enterprise' },
]

export function TenantDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { isSuperAdmin, isFinance } = useAuth()
  const { data: tenant, isLoading } = useTenantDetail(id ?? '')

  // Plan değiştir
  const [selectedPlanName, setSelectedPlanName] = useState('')
  const [planLoading, setPlanLoading] = useState(false)

  // Suspend
  const [suspendDialogOpen, setSuspendDialogOpen] = useState(false)
  const [suspendLoading, setSuspendLoading] = useState(false)

  // Not ekleme
  const [noteContent, setNoteContent] = useState('')
  const [noteLoading, setNoteLoading] = useState(false)

  const canChangePlan = isSuperAdmin || isFinance
  const canSuspend = isSuperAdmin

  const invalidateTenant = () => {
    queryClient.invalidateQueries({ queryKey: ['tenant', id] })
  }

  async function handlePlanUpdate() {
    if (!selectedPlanName || !id) return
    // Backend expects a PlanId guid — for now we fetch plans to match by name
    // Simple approach: send planName as text and let backend resolve
    // Backend UpdateTenantPlanCommand takes PlanId guid
    // We need to get the plan ID from the pricing endpoint
    try {
      setPlanLoading(true)
      const plansRes = await api.get<{ id: string; name: string }[]>('/api/platform/pricing')
      const plan = plansRes.data.find(
        (p) => p.name.toLowerCase() === selectedPlanName.toLowerCase(),
      )
      if (!plan) {
        toast.error('Plan bulunamadı.')
        return
      }
      await api.put(`/api/platform/tenants/${id}/plan`, { planId: plan.id })
      toast.success('Plan güncellendi.')
      invalidateTenant()
    } catch {
      toast.error('Plan güncellenirken hata oluştu.')
    } finally {
      setPlanLoading(false)
    }
  }

  async function handleSuspend() {
    if (!id) return
    const willSuspend = tenant?.planStatus !== 'Suspended'
    try {
      setSuspendLoading(true)
      await api.put(`/api/platform/tenants/${id}/suspend`, { suspend: willSuspend })
      toast.success(willSuspend ? 'Tenant askıya alındı.' : 'Tenant aktif edildi.')
      setSuspendDialogOpen(false)
      invalidateTenant()
    } catch {
      toast.error('İşlem sırasında hata oluştu.')
    } finally {
      setSuspendLoading(false)
    }
  }

  async function handleAddNote() {
    if (!noteContent.trim() || !id) return
    try {
      setNoteLoading(true)
      await api.post(`/api/platform/tenants/${id}/notes`, { content: noteContent.trim() })
      toast.success('Not eklendi.')
      setNoteContent('')
      invalidateTenant()
    } catch {
      toast.error('Not eklenirken hata oluştu.')
    } finally {
      setNoteLoading(false)
    }
  }

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
  const isSuspended = tenant.planStatus === 'Suspended'

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
          <TabsTrigger value="actions">Aksiyonlar</TabsTrigger>
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
                  💡 Ödeme detayları ve fatura geçmişi <strong>Faz 7 (İyzico)</strong> tamamlandıktan sonra aktif olacak.
                </p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Aksiyonlar */}
        <TabsContent value="actions">
          <div className="grid gap-4 md:grid-cols-2">
            {/* Plan değiştir */}
            {canChangePlan ? (
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Plan Değiştir</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <p className="text-xs text-muted-foreground mb-1">Mevcut Plan</p>
                    <p className="text-sm font-medium">{tenant.planName}</p>
                  </div>
                  <div className="space-y-2">
                    <p className="text-xs text-muted-foreground">Yeni Plan</p>
                    <Select onValueChange={setSelectedPlanName} value={selectedPlanName}>
                      <SelectTrigger>
                        <SelectValue placeholder="Plan seçin..." />
                      </SelectTrigger>
                      <SelectContent>
                        {PLAN_OPTIONS.map((p) => (
                          <SelectItem key={p.value} value={p.value}>{p.label}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <Button
                    onClick={handlePlanUpdate}
                    disabled={!selectedPlanName || planLoading}
                    className="w-full"
                  >
                    {planLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    Planı Güncelle
                  </Button>
                </CardContent>
              </Card>
            ) : (
              <Card className="opacity-50">
                <CardHeader>
                  <CardTitle className="text-base">Plan Değiştir</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground">Bu işlem için yetkiniz yok.</p>
                </CardContent>
              </Card>
            )}

            {/* Hesap Durumu */}
            {canSuspend ? (
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Hesap Durumu</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <p className="text-xs text-muted-foreground mb-1">Mevcut Durum</p>
                    <span className={`rounded-full border px-2 py-0.5 text-xs font-medium ${badge.class}`}>
                      {badge.label}
                    </span>
                  </div>
                  {isSuspended ? (
                    <Button
                      variant="outline"
                      className="w-full border-emerald-500/30 text-emerald-400 hover:bg-emerald-500/10"
                      onClick={() => setSuspendDialogOpen(true)}
                    >
                      Aktif Et
                    </Button>
                  ) : (
                    <Button
                      variant="outline"
                      className="w-full border-destructive/30 text-destructive hover:bg-destructive/10"
                      onClick={() => setSuspendDialogOpen(true)}
                    >
                      <AlertTriangle className="mr-2 h-4 w-4" />
                      Askıya Al
                    </Button>
                  )}
                </CardContent>
              </Card>
            ) : (
              <Card className="opacity-50">
                <CardHeader>
                  <CardTitle className="text-base">Hesap Durumu</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground">Bu işlem için yetkiniz yok.</p>
                </CardContent>
              </Card>
            )}
          </div>
        </TabsContent>

        {/* Notlar */}
        <TabsContent value="notes">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Platform Notları</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Not ekleme formu */}
              <div className="space-y-2">
                <Textarea
                  placeholder="İç not ekleyin... (müşteriye görünmez)"
                  value={noteContent}
                  onChange={(e) => setNoteContent(e.target.value)}
                  rows={3}
                />
                <Button
                  onClick={handleAddNote}
                  disabled={!noteContent.trim() || noteLoading}
                  size="sm"
                >
                  {noteLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Not Ekle
                </Button>
              </div>

              <div className="border-t border-border pt-4">
                {tenant.notes.length === 0 ? (
                  <div className="py-6 text-center text-muted-foreground">
                    <MessageSquare className="mx-auto mb-2 h-8 w-8 opacity-40" />
                    <p className="text-sm">Henüz not eklenmemiş.</p>
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
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Suspend onay dialog */}
      <Dialog open={suspendDialogOpen} onOpenChange={setSuspendDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {isSuspended ? 'Tenant\'ı Aktif Et' : 'Tenant\'ı Askıya Al'}
            </DialogTitle>
            <DialogDescription>
              {isSuspended
                ? `"${tenant.name}" tenant'ını aktif etmek istediğinizden emin misiniz? Kullanıcılar tekrar giriş yapabilecek.`
                : `"${tenant.name}" tenant'ını askıya almak istediğinizden emin misiniz? Tüm kullanıcıların erişimi kesilecek.`}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setSuspendDialogOpen(false)}>
              İptal
            </Button>
            <Button
              variant={isSuspended ? 'default' : 'destructive'}
              onClick={handleSuspend}
              disabled={suspendLoading}
            >
              {suspendLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {isSuspended ? 'Aktif Et' : 'Askıya Al'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
