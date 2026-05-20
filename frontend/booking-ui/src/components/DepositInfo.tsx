import { Info } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';

interface DepositInfoProps {
  amount: number;
}

export function DepositInfo({ amount }: DepositInfoProps) {
  return (
    <Card className="border-blue-200 bg-blue-50">
      <CardContent className="pt-6">
        <div className="flex items-start gap-3">
          <Info className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
          <div>
            <h3 className="font-semibold text-blue-900 mb-1">
              Ön Ödeme Gerekli
            </h3>
            <p className="text-sm text-blue-800">
              Rezervasyonunuzu tamamlamak için{' '}
              <span className="font-bold">₺{amount}</span> tutarında ön ödeme
              yapmanız gerekmektedir.
            </p>
            <p className="text-xs text-blue-700 mt-2">
              Ödeme işlemi güvenli ödeme sağlayıcımız üzerinden
              gerçekleştirilecektir.
            </p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
