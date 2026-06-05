import { useState } from 'react'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { ChevronLeft, ChevronRight, MessageCircle } from 'lucide-react'
import { useWhatsAppMessageHistory } from '@/hooks/useWhatsAppSettings'
import type { WhatsAppStatus, WhatsAppTemplate } from '@/types/whatsapp'

const STATUS_LABELS: Record<WhatsAppStatus, string> = {
  Queued: 'Kuyrukta',
  Sent: 'Gönderildi',
  Delivered: 'İletildi',
  Read: 'Okundu',
  Failed: 'Başarısız',
}

const STATUS_COLORS: Record<WhatsAppStatus, string> = {
  Queued: 'text-gray-600 bg-gray-100 border-gray-200',
  Sent: 'text-blue-600 bg-blue-50 border-blue-200',
  Delivered: 'text-green-600 bg-green-50 border-green-200',
  Read: 'text-indigo-600 bg-indigo-50 border-indigo-200',
  Failed: 'text-red-600 bg-red-50 border-red-200',
}

const TEMPLATE_LABELS: Record<WhatsAppTemplate, string> = {
  ReservationReceived: 'Rezervasyon Alındı',
  ReservationConfirmed: 'Rezervasyon Onaylandı',
  Reminder: 'Hatırlatma',
  Cancellation: 'İptal',
}

const PAGE_SIZE = 15

export function WhatsAppMessageHistory() {
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<WhatsAppStatus | 'all'>('all')

  const { data, isLoading } = useWhatsAppMessageHistory({
    page,
    pageSize: PAGE_SIZE,
    status: statusFilter !== 'all' ? statusFilter : undefined,
  })

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <MessageCircle className="h-4 w-4 text-green-600" />
            <CardTitle className="text-base">Mesaj Geçmişi</CardTitle>
          </div>
          {data && (
            <span className="text-xs text-gray-500">{data.totalCount} mesaj</span>
          )}
        </div>
        <CardDescription>Son gönderilen WhatsApp bildirimleri.</CardDescription>
      </CardHeader>

      <CardContent className="space-y-4">
        {/* Filtre */}
        <div className="flex gap-2">
          <Select
            value={statusFilter}
            onValueChange={(v) => {
              setStatusFilter(v as WhatsAppStatus | 'all')
              setPage(1)
            }}
          >
            <SelectTrigger className="w-40 h-8 text-xs">
              <SelectValue placeholder="Durum" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tümü</SelectItem>
              {(Object.keys(STATUS_LABELS) as WhatsAppStatus[]).map((s) => (
                <SelectItem key={s} value={s}>
                  {STATUS_LABELS[s]}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Tablo */}
        {isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 5 }).map((_, i) => (
              <Skeleton key={i} className="h-10 w-full" />
            ))}
          </div>
        ) : !data || data.items.length === 0 ? (
          <div className="text-center py-10 text-gray-500 text-sm">
            Henüz WhatsApp mesajı gönderilmedi.
          </div>
        ) : (
          <div className="overflow-x-auto rounded-md border">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 border-b">
                <tr>
                  <th className="text-left px-4 py-2.5 font-medium text-gray-600">Alıcı</th>
                  <th className="text-left px-4 py-2.5 font-medium text-gray-600">Tür</th>
                  <th className="text-left px-4 py-2.5 font-medium text-gray-600">Durum</th>
                  <th className="text-left px-4 py-2.5 font-medium text-gray-600">Gönderildi</th>
                  <th className="text-left px-4 py-2.5 font-medium text-gray-600">İletildi</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.items.map((msg) => (
                  <tr key={msg.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-mono text-xs text-gray-700">{msg.toPhone}</td>
                    <td className="px-4 py-3 text-gray-700">
                      {TEMPLATE_LABELS[msg.template] ?? msg.template}
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium border ${STATUS_COLORS[msg.status]}`}
                      >
                        {STATUS_LABELS[msg.status]}
                      </span>
                      {msg.status === 'Failed' && msg.errorMessage && (
                        <p className="text-xs text-red-500 mt-0.5 max-w-xs truncate" title={msg.errorMessage}>
                          {msg.errorMessage}
                        </p>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-500 text-xs">
                      {msg.sentAt
                        ? format(new Date(msg.sentAt), 'd MMM HH:mm', { locale: tr })
                        : '—'}
                    </td>
                    <td className="px-4 py-3 text-gray-500 text-xs">
                      {msg.deliveredAt
                        ? format(new Date(msg.deliveredAt), 'd MMM HH:mm', { locale: tr })
                        : '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between pt-2">
            <span className="text-xs text-gray-500">
              Sayfa {data.page} / {data.totalPages}
            </span>
            <div className="flex gap-1">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => p - 1)}
                disabled={!data.hasPreviousPage}
                className="h-7 w-7 p-0"
              >
                <ChevronLeft className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => p + 1)}
                disabled={!data.hasNextPage}
                className="h-7 w-7 p-0"
              >
                <ChevronRight className="h-3.5 w-3.5" />
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
