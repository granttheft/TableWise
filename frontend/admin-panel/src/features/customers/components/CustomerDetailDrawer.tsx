import { useState, useEffect } from 'react'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { Switch } from '@/components/ui/switch'
import { Label } from '@/components/ui/label'
import { Separator } from '@/components/ui/separator'
import { Mail, Phone, Calendar, MapPin, Shield } from 'lucide-react'
import { 
  useCustomer, 
  useUpdateCustomerTier, 
  useUpdateCustomerBlacklist, 
  useUpdateCustomerNotes 
} from '@/hooks/useCustomers'
import { useReservations } from '@/hooks/useReservations'
import { getTierLabel, getTierColor, getStatusLabel, getStatusColor } from '@/features/reservations/utils/reservationHelpers'
import type { CustomerTier } from '@/types/api'

interface CustomerDetailDrawerProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  customerId: string
}

export function CustomerDetailDrawer({ open, onOpenChange, customerId }: CustomerDetailDrawerProps) {
  const { data: customer, isLoading } = useCustomer(customerId)
  const { data: reservations = [] } = useReservations({})
  
  const updateTier = useUpdateCustomerTier()
  const updateBlacklist = useUpdateCustomerBlacklist()
  const updateNotes = useUpdateCustomerNotes()

  const [tier, setTier] = useState<CustomerTier>('Regular')
  const [isBlacklisted, setIsBlacklisted] = useState(false)
  const [blacklistReason, setBlacklistReason] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (customer) {
      setTier(customer.tier)
      setIsBlacklisted(customer.isBlacklisted)
      setBlacklistReason(customer.blacklistReason || '')
      setNotes(customer.notes || '')
    }
  }, [customer])

  const handleSave = async () => {
    if (!customer) return

    try {
      // Update tier if changed
      if (tier !== customer.tier) {
        await updateTier.mutateAsync({ customerId: customer.id, tier })
      }

      // Update blacklist if changed
      if (isBlacklisted !== customer.isBlacklisted || blacklistReason !== (customer.blacklistReason || '')) {
        await updateBlacklist.mutateAsync({
          customerId: customer.id,
          isBlacklisted,
          blacklistReason,
        })
      }

      // Update notes if changed
      if (notes !== (customer.notes || '')) {
        await updateNotes.mutateAsync({ customerId: customer.id, notes })
      }
    } catch (error) {
      // Errors are handled in mutation hooks
    }
  }

  if (isLoading) {
    return (
      <Sheet open={open} onOpenChange={onOpenChange}>
        <SheetContent className="w-full sm:max-w-lg overflow-y-auto">
          <div className="flex items-center justify-center py-12">
            <div className="text-muted-foreground">Yükleniyor...</div>
          </div>
        </SheetContent>
      </Sheet>
    )
  }

  if (!customer) {
    return null
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="w-full sm:max-w-lg overflow-y-auto">
        <SheetHeader>
          <SheetTitle>{customer.fullName}</SheetTitle>
          <SheetDescription>Müşteri detayları ve rezervasyon geçmişi</SheetDescription>
        </SheetHeader>

        <div className="space-y-6 py-6">
          {/* Contact Info */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base">İletişim Bilgileri</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {customer.email && (
                <div className="flex items-center gap-2 text-sm">
                  <Mail className="h-4 w-4 text-muted-foreground" />
                  <span>{customer.email}</span>
                </div>
              )}
              {customer.phone && (
                <div className="flex items-center gap-2 text-sm">
                  <Phone className="h-4 w-4 text-muted-foreground" />
                  <span>{customer.phone}</span>
                </div>
              )}
              <div className="flex items-center gap-2 text-sm">
                <Calendar className="h-4 w-4 text-muted-foreground" />
                <span>İlk Rezervasyon: {new Date(customer.createdAt).toLocaleDateString('tr-TR')}</span>
              </div>
              <div className="flex items-center gap-2 text-sm">
                <MapPin className="h-4 w-4 text-muted-foreground" />
                <span>Toplam Ziyaret: {customer.totalVisits || 0}</span>
              </div>
            </CardContent>
          </Card>

          {/* Tier Management (Owner-only) */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base flex items-center gap-2">
                <Shield className="h-4 w-4" />
                Müşteri Tier
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="space-y-2">
                <Label>Tier Seviyesi</Label>
                <Select value={tier} onValueChange={setTier}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="VIP">VIP</SelectItem>
                    <SelectItem value="Regular">Regular</SelectItem>
                    <SelectItem value="New">New</SelectItem>
                  </SelectContent>
                </Select>
                <p className="text-sm text-muted-foreground">
                  Tier seviyesi kural motorunda kullanılır (VIP öncelik, indirim vb.)
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Blacklist */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Blacklist</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex items-center space-x-2">
                <Switch checked={isBlacklisted} onCheckedChange={setIsBlacklisted} />
                <Label className="cursor-pointer" onClick={() => setIsBlacklisted(!isBlacklisted)}>
                  Bu müşteriyi blacklist'e ekle
                </Label>
              </div>

              {isBlacklisted && (
                <div className="space-y-2">
                  <Label>Blacklist Sebebi</Label>
                  <Textarea
                    value={blacklistReason}
                    onChange={(e) => setBlacklistReason(e.target.value)}
                    placeholder="Neden blacklist'e eklendi?"
                    rows={3}
                  />
                </div>
              )}
            </CardContent>
          </Card>

          {/* Internal Notes */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base">İç Notlar</CardTitle>
            </CardHeader>
            <CardContent>
              <Textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder="Müşteri hakkında notlarınız..."
                rows={4}
              />
            </CardContent>
          </Card>

          {/* Reservation History */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Rezervasyon Geçmişi</CardTitle>
            </CardHeader>
            <CardContent>
              {reservations.length === 0 ? (
                <p className="text-center text-sm text-muted-foreground py-4">
                  Henüz rezervasyon yok
                </p>
              ) : (
                <div className="space-y-3">
                  {reservations
                    .filter((r) => r.customerId === customerId)
                    .slice(0, 10)
                    .map((reservation) => (
                      <div key={reservation.id} className="flex items-start justify-between border-b pb-3 last:border-0">
                        <div className="space-y-1">
                          <div className="text-sm font-medium">
                            {new Date(reservation.date).toLocaleDateString('tr-TR')} • {reservation.timeSlot}
                          </div>
                          <div className="text-sm text-muted-foreground">
                            {reservation.guestCount} kişi
                          </div>
                        </div>
                        <Badge variant="secondary" className={getStatusColor(reservation.status)}>
                          {getStatusLabel(reservation.status)}
                        </Badge>
                      </div>
                    ))}
                </div>
              )}
            </CardContent>
          </Card>

          <Separator />

          <Button onClick={handleSave} className="w-full">
            Değişiklikleri Kaydet
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  )
}
