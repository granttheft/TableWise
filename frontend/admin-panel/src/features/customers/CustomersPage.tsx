import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Badge } from '@/components/ui/badge'
import { Card } from '@/components/ui/card'
import { Search, UserX } from 'lucide-react'
import { CustomerDetailDrawer } from './components/CustomerDetailDrawer'
import { useCustomers } from '@/hooks/useCustomers'
import { getTierLabel, getTierColor } from '@/features/reservations/utils/reservationHelpers'

export function CustomersPage() {
  const [searchTerm, setSearchTerm] = useState('')
  const [tierFilter, setTierFilter] = useState<string>('all')
  const [blacklistFilter, setBlacklistFilter] = useState<string>('all')
  const [selectedCustomerId, setSelectedCustomerId] = useState<string | null>(null)
  const [isDrawerOpen, setIsDrawerOpen] = useState(false)

  const { data: customers = [], isLoading } = useCustomers({
    searchTerm: searchTerm || undefined,
    tier: tierFilter !== 'all' ? tierFilter : undefined,
    isBlacklisted: blacklistFilter === 'all' ? undefined : blacklistFilter === 'true',
  })

  const handleCustomerClick = (customerId: string) => {
    setSelectedCustomerId(customerId)
    setIsDrawerOpen(true)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Müşteriler</h1>
        <p className="text-muted-foreground">Müşteri listesi ve geçmişi</p>
      </div>

      {/* Filters */}
      <Card className="p-4">
        <div className="flex flex-wrap gap-4">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="İsim, email veya telefon ile ara..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-9"
            />
          </div>

          <Select value={tierFilter} onValueChange={setTierFilter}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Tier Filtresi" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tüm Tier'lar</SelectItem>
              <SelectItem value="VIP">VIP</SelectItem>
              <SelectItem value="Regular">Regular</SelectItem>
              <SelectItem value="New">New</SelectItem>
            </SelectContent>
          </Select>

          <Select value={blacklistFilter} onValueChange={setBlacklistFilter}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Blacklist" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tümü</SelectItem>
              <SelectItem value="false">Aktif Müşteriler</SelectItem>
              <SelectItem value="true">Blacklist</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </Card>

      {/* Customers Table */}
      {isLoading ? (
        <Card className="flex items-center justify-center py-12">
          <div className="text-muted-foreground">Yükleniyor...</div>
        </Card>
      ) : customers.length === 0 ? (
        <Card className="flex flex-col items-center justify-center py-12">
          <UserX className="mb-4 h-12 w-12 text-muted-foreground" />
          <p className="text-muted-foreground">
            {searchTerm || tierFilter !== 'all' || blacklistFilter !== 'all'
              ? 'Filtrelere uygun müşteri bulunamadı'
              : 'Henüz müşteriniz yok'}
          </p>
        </Card>
      ) : (
        <Card>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="border-b">
                <tr>
                  <th className="px-4 py-3 text-left text-sm font-medium">İsim</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">Email</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">Telefon</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">Tier</th>
                  <th className="px-4 py-3 text-right text-sm font-medium">Toplam Ziyaret</th>
                  <th className="px-4 py-3 text-left text-sm font-medium">Son Rezervasyon</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {customers.map((customer) => (
                  <tr
                    key={customer.id}
                    onClick={() => handleCustomerClick(customer.id)}
                    className={`cursor-pointer hover:bg-muted transition-colors ${
                      customer.isBlacklisted ? 'opacity-60' : ''
                    }`}
                  >
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <span className="font-medium">{customer.fullName}</span>
                        {customer.isBlacklisted && (
                          <Badge variant="destructive" className="text-xs">
                            Blacklist
                          </Badge>
                        )}
                      </div>
                    </td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">{customer.email || '-'}</td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">{customer.phone || '-'}</td>
                    <td className="px-4 py-3">
                      <Badge variant="secondary" className={getTierColor(customer.tier)}>
                        {getTierLabel(customer.tier)}
                      </Badge>
                    </td>
                    <td className="px-4 py-3 text-right font-medium">{customer.totalVisits || 0}</td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">
                      {customer.lastReservationDate
                        ? new Date(customer.lastReservationDate).toLocaleDateString('tr-TR')
                        : '-'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {/* Customer Detail Drawer */}
      {selectedCustomerId && (
        <CustomerDetailDrawer
          open={isDrawerOpen}
          onOpenChange={setIsDrawerOpen}
          customerId={selectedCustomerId}
        />
      )}
    </div>
  )
}
