import { useState } from 'react'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'
import { Textarea } from '@/components/ui/textarea'
import {
  User,
  Users,
  Calendar,
  Clock,
  MessageSquare,
  FileText,
  Tag,
  CreditCard,
  Copy,
  CheckCircle,
  XCircle,
  Ban,
  Edit,
} from 'lucide-react'
import { useReservation, useReservationStatusLog, useUpdateReservation, useUpdateReservationStatus } from '@/hooks/useReservations'
import { Skeleton } from '@/components/ui/skeleton'
import { toast } from 'sonner'
import type { ReservationStatus } from '@/types/api'
import {
  getStatusLabel,
  getStatusVariant,
  getTierLabel,
  getTierColor,
  formatDate,
  formatTime,
  formatConfirmCode,
} from '../utils/reservationHelpers'
import { getRuleTypeLabel } from '@/features/rules/utils/ruleHumanReadable'
import { tr } from 'date-fns/locale'

interface ReservationDetailDrawerProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  reservationId: string | null
}

export function ReservationDetailDrawer({
  open,
  onOpenChange,
  reservationId,
}: ReservationDetailDrawerProps) {
  const [internalNotes, setInternalNotes] = useState('')
  const [isEditingNotes, setIsEditingNotes] = useState(false)

  const { data: reservation, isLoading } = useReservation(reservationId || undefined)
  const { data: statusLog = [] } = useReservationStatusLog(reservationId || undefined)
  const updateReservationMutation = useUpdateReservation()
  const updateStatusMutation = useUpdateReservationStatus()

  const handleUpdateNotes = () => {
    if (!reservation) return
    updateReservationMutation.mutate(
      {
        reservationId: reservation.id,
        updates: { internalNotes },
      },
      {
        onSuccess: () => {
          setIsEditingNotes(false)
          toast.success('Notlar güncellendi')
        },
      }
    )
  }

  const handleUpdateStatus = (status: ReservationStatus) => {
    if (!reservation) return
    if (!confirm(`Rezervasyon durumunu "${getStatusLabel(status)}" olarak değiştirmek istediğinizden emin misiniz?`)) return

    updateStatusMutation.mutate({
      reservationId: reservation.id,
      status,
    })
  }

  const handleCopyConfirmCode = () => {
    if (!reservation) return
    navigator.clipboard.writeText(reservation.confirmCode)
    toast.success('Onay kodu kopyalandı')
  }

  if (isLoading) {
    return (
      <Sheet open={open} onOpenChange={onOpenChange}>
        <SheetContent className="w-full sm:max-w-lg overflow-y-auto">
          <SheetHeader>
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-4 w-64" />
          </SheetHeader>
          <div className="space-y-4 mt-6">
            {[1, 2, 3, 4, 5].map((i) => (
              <Skeleton key={i} className="h-24" />
            ))}
          </div>
        </SheetContent>
      </Sheet>
    )
  }

  if (!reservation) {
    return null
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="w-full sm:max-w-lg overflow-y-auto">
        <SheetHeader>
          <SheetTitle>Rezervasyon Detayı</SheetTitle>
          <SheetDescription>
            {formatDate(reservation.reservedFor, 'PPP')} - {formatTime(reservation.reservedFor)}
          </SheetDescription>
        </SheetHeader>

        <div className="space-y-6 mt-6">
          {/* Status & Actions */}
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center justify-between mb-4">
                <Badge variant={getStatusVariant(reservation.status as ReservationStatus)} className="text-sm">
                  {getStatusLabel(reservation.status as ReservationStatus)}
                </Badge>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={handleCopyConfirmCode}
                >
                  <Copy className="h-4 w-4 mr-1" />
                  {formatConfirmCode(reservation.confirmCode)}
                </Button>
              </div>

              <div className="flex flex-wrap gap-2">
                {reservation.status === 'Confirmed' && (
                  <>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => handleUpdateStatus('Completed')}
                    >
                      <CheckCircle className="h-4 w-4 mr-1" />
                      Tamamlandı
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => handleUpdateStatus('NoShow')}
                    >
                      <XCircle className="h-4 w-4 mr-1" />
                      Gelmedi
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => handleUpdateStatus('Cancelled')}
                    >
                      <Ban className="h-4 w-4 mr-1" />
                      İptal
                    </Button>
                  </>
                )}
                <Button size="sm" variant="outline">
                  <Edit className="h-4 w-4 mr-1" />
                  Düzenle
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Customer Info */}
          <Card>
            <CardContent className="p-4 space-y-3">
              <h3 className="font-semibold flex items-center gap-2">
                <User className="h-4 w-4" />
                Müşteri Bilgileri
              </h3>
              <div className="space-y-2 text-sm">
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Misafir:</span>
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{reservation.guestName}</span>
                    {reservation.customerTier && (
                      <Badge className={getTierColor(reservation.customerTier)}>
                        {getTierLabel(reservation.customerTier)}
                      </Badge>
                    )}
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Email:</span>
                  <span className="font-medium">{reservation.guestEmail ?? '—'}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Telefon:</span>
                  <span className="font-medium">{reservation.guestPhone}</span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Reservation Info */}
          <Card>
            <CardContent className="p-4 space-y-3">
              <h3 className="font-semibold flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Rezervasyon Detayları
              </h3>
              <div className="space-y-2 text-sm">
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Tarih:</span>
                  <span className="font-medium">{formatDate(reservation.reservedFor, 'PPP')}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Saat:</span>
                  <span className="font-medium">
                    {formatTime(reservation.reservedFor)} - {formatTime(reservation.endTime)}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Masa:</span>
                  <span className="font-medium">{reservation.tableName ?? '—'}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Kişi Sayısı:</span>
                  <span className="font-medium flex items-center gap-1">
                    <Users className="h-3 w-3" />
                    {reservation.partySize}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Süre:</span>
                  <span className="font-medium">
                    {differenceInMinutes(parseISO(reservation.endTime), parseISO(reservation.reservedFor))} dakika
                  </span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Special Requests */}
          {reservation.specialRequests && (
            <Card>
              <CardContent className="p-4">
                <h3 className="font-semibold flex items-center gap-2 mb-2">
                  <MessageSquare className="h-4 w-4" />
                  Özel İstekler
                </h3>
                <p className="text-sm text-muted-foreground">{reservation.specialRequests}</p>
              </CardContent>
            </Card>
          )}

          {/* Applied Rules */}
          {reservation.appliedRules && reservation.appliedRules.length > 0 && (
            <Card>
              <CardContent className="p-4">
                <h3 className="font-semibold flex items-center gap-2 mb-3">
                  <Tag className="h-4 w-4" />
                  Uygulanan Kurallar
                </h3>
                <div className="flex flex-wrap gap-2">
                  {reservation.appliedRules.map((rule, index) => {
                    const label = getRuleTypeLabel(rule)
                    const isGroupComposition =
                      rule === 'group_composition' ||
                      rule === 'GroupComposition' ||
                      label.toLowerCase().includes('grup dengesi')
                    return (
                      <Badge
                        key={index}
                        variant="secondary"
                        className={
                          isGroupComposition
                            ? 'border border-pink-500/40 bg-pink-500/10 text-pink-800 dark:text-pink-100'
                            : undefined
                        }
                      >
                        <span className="mr-1" aria-hidden>
                          {isGroupComposition ? '⚖' : null}
                        </span>
                        {label}
                      </Badge>
                    )
                  })}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Discount & Deposit */}
          {(reservation.discountPercent || reservation.depositAmount) && (
            <Card>
              <CardContent className="p-4 space-y-3">
                <h3 className="font-semibold flex items-center gap-2">
                  <CreditCard className="h-4 w-4" />
                  Ödeme Bilgileri
                </h3>
                <div className="space-y-2 text-sm">
                  {reservation.discountPercent && (
                    <div className="flex items-center justify-between">
                      <span className="text-muted-foreground">İndirim:</span>
                      <Badge variant="secondary">%{reservation.discountPercent}</Badge>
                    </div>
                  )}
                  {reservation.depositAmount && (
                    <>
                      <div className="flex items-center justify-between">
                        <span className="text-muted-foreground">Kapora:</span>
                        <span className="font-medium">₺{reservation.depositAmount}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-muted-foreground">Durum:</span>
                        <Badge variant={reservation.depositStatus === 'Paid' ? 'default' : 'outline'}>
                          {reservation.depositStatus === 'Paid'
                            ? 'Ödendi'
                            : reservation.depositStatus === 'Refunded'
                              ? 'İade Edildi'
                              : 'Bekleniyor'}
                        </Badge>
                      </div>
                    </>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Internal Notes */}
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center justify-between mb-3">
                <h3 className="font-semibold flex items-center gap-2">
                  <FileText className="h-4 w-4" />
                  Dahili Notlar
                </h3>
                {!isEditingNotes && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => {
                      setInternalNotes(reservation.internalNotes || '')
                      setIsEditingNotes(true)
                    }}
                  >
                    <Edit className="h-4 w-4" />
                  </Button>
                )}
              </div>
              {isEditingNotes ? (
                <div className="space-y-2">
                  <Textarea
                    value={internalNotes}
                    onChange={(e) => setInternalNotes(e.target.value)}
                    rows={4}
                    placeholder="Dahili notlar ekleyin..."
                  />
                  <div className="flex gap-2">
                    <Button size="sm" onClick={handleUpdateNotes}>
                      Kaydet
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => setIsEditingNotes(false)}
                    >
                      İptal
                    </Button>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">
                  {reservation.internalNotes || 'Henüz not eklenmedi'}
                </p>
              )}
            </CardContent>
          </Card>

          {/* Status Log */}
          {statusLog.length > 0 && (
            <Card>
              <CardContent className="p-4">
                <h3 className="font-semibold flex items-center gap-2 mb-3">
                  <Clock className="h-4 w-4" />
                  Durum Geçmişi
                </h3>
                <div className="space-y-3">
                  {statusLog.map((log) => (
                    <div key={log.id} className="flex items-start gap-3 text-sm">
                      <div className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-accent text-accent-foreground text-xs">
                        →
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          {log.fromStatus && (
                            <Badge variant="outline" className="text-xs">
                              {getStatusLabel(log.fromStatus)}
                            </Badge>
                          )}
                          <span className="text-muted-foreground">→</span>
                          <Badge variant={getStatusVariant(log.toStatus)} className="text-xs">
                            {getStatusLabel(log.toStatus)}
                          </Badge>
                        </div>
                        <div className="mt-1 text-xs text-muted-foreground">
                          {log.userName} •{' '}
                          {formatDistanceToNow(parseISO(log.timestamp), {
                            addSuffix: true,
                            locale: tr,
                          })}
                        </div>
                        {log.reason && (
                          <div className="mt-1 text-xs text-muted-foreground italic">
                            "{log.reason}"
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Meta Info */}
          <div className="text-xs text-muted-foreground space-y-1 pb-4">
            <div>
              Oluşturulma: {formatDate(reservation.createdAt, 'PPP p')}
            </div>
            <div>
              Son Güncelleme: {formatDate(reservation.updatedAt ?? reservation.createdAt, 'PPP p')}
            </div>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  )
}
