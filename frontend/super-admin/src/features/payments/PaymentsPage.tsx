import { Receipt } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'

export function PaymentsPage() {
  return (
    <div className="flex h-full items-center justify-center p-6">
      <Card className="max-w-md w-full">
        <CardContent className="flex flex-col items-center gap-4 py-12 text-center">
          <div className="rounded-full bg-muted p-4">
            <Receipt className="h-8 w-8 text-muted-foreground" />
          </div>
          <div>
            <h2 className="text-lg font-semibold">Ödeme Takibi</h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Ödeme takibi, Faz 7 (İyzico entegrasyonu) tamamlandığında aktif olacak.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
