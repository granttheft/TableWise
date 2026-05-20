import { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  useReservationDetail,
  useCancelReservation,
} from '@/hooks/useReservationDetail';
import { formatDate, formatTime } from '@/lib/utils';
import { Loader2, ArrowLeft, AlertTriangle } from 'lucide-react';
import { toast } from 'sonner';
import { handleApiError } from '@/lib/api';

const cancelReasons = [
  'Planlarım değişti',
  'Farklı bir mekan seçtim',
  'Yanlışlıkla rezervasyon yaptım',
  'Tarih uygun değil',
  'Diğer',
];

export function CancelPage() {
  const { code } = useParams<{ code: string }>();
  const navigate = useNavigate();
  const [reason, setReason] = useState<string>('');

  const { data: reservation, isLoading } = useReservationDetail(code!);
  const cancelMutation = useCancelReservation(code!);

  const handleCancel = async () => {
    try {
      await cancelMutation.mutateAsync({ reason: reason || undefined });
      toast.success('Rezervasyonunuz iptal edildi');
      navigate(`/rezervasyon/goruntule/${code}`);
    } catch (err) {
      const apiError = handleApiError(err);
      toast.error(apiError.message || 'İptal işlemi başarısız');
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!reservation) {
    return (
      <div className="min-h-screen flex items-center justify-center px-4">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-600 mb-2">Hata</h1>
          <p className="text-muted-foreground mb-4">
            Rezervasyon bulunamadı.
          </p>
          <Link to="/">
            <Button>Ana Sayfaya Dön</Button>
          </Link>
        </div>
      </div>
    );
  }

  if (reservation.status === 'cancelled') {
    return (
      <div className="min-h-screen flex items-center justify-center px-4">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 mb-2">
            Bu rezervasyon zaten iptal edilmiş
          </h1>
          <Link to={`/rezervasyon/goruntule/${code}`}>
            <Button>Rezervasyonu Görüntüle</Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <>
      <Helmet>
        <title>Rezervasyon İptali | Tablewise</title>
        <meta name="description" content="Rezervasyonunuzu iptal edin" />
      </Helmet>

      <div className="min-h-screen bg-gray-50">
        <div className="max-w-2xl mx-auto px-4 py-8">
          <Link to={`/rezervasyon/goruntule/${code}`}>
            <Button variant="ghost" className="mb-6">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Geri
            </Button>
          </Link>

          <div className="space-y-6">
            <div>
              <h1 className="text-2xl font-bold mb-2">Rezervasyon İptali</h1>
              <p className="text-muted-foreground">
                Rezervasyonunuzu iptal etmek üzeresiniz
              </p>
            </div>

            {/* Warning */}
            <Card className="border-yellow-200 bg-yellow-50">
              <CardContent className="pt-6">
                <div className="flex items-start gap-3">
                  <AlertTriangle className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                  <div>
                    <h3 className="font-semibold text-yellow-900 mb-1">
                      Dikkat
                    </h3>
                    <p className="text-sm text-yellow-800">
                      Bu işlem geri alınamaz. Rezervasyonunuz iptal edildiğinde
                      masa müsaitliği tekrar açılacaktır.
                    </p>
                    {reservation.depositPaid && (
                      <p className="text-sm text-yellow-800 mt-2">
                        Ödediğiniz kapora tutarı iptal politikasına göre iade
                        edilecektir.
                      </p>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Reservation summary */}
            <Card>
              <CardHeader>
                <CardTitle>Rezervasyon Bilgileri</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Mekan</p>
                    <p className="font-medium">{reservation.venueName}</p>
                  </div>

                  <div>
                    <p className="text-sm text-muted-foreground">Kişi Sayısı</p>
                    <p className="font-medium">{reservation.partySize} Kişi</p>
                  </div>

                  <div>
                    <p className="text-sm text-muted-foreground">Tarih</p>
                    <p className="font-medium">
                      {formatDate(reservation.date)}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-muted-foreground">Saat</p>
                    <p className="font-medium">
                      {formatTime(reservation.time)}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Cancel reason */}
            <Card>
              <CardHeader>
                <CardTitle>İptal Sebebi (Opsiyonel)</CardTitle>
              </CardHeader>
              <CardContent>
                <Label htmlFor="reason">
                  Neden iptal etmek istiyorsunuz?
                </Label>
                <Select value={reason} onValueChange={setReason}>
                  <SelectTrigger id="reason" className="mt-2">
                    <SelectValue placeholder="Bir sebep seçin..." />
                  </SelectTrigger>
                  <SelectContent>
                    {cancelReasons.map((r) => (
                      <SelectItem key={r} value={r}>
                        {r}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </CardContent>
            </Card>

            {/* Actions */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <Link to={`/rezervasyon/goruntule/${code}`}>
                <Button variant="outline" className="w-full">
                  Vazgeç
                </Button>
              </Link>

              <Button
                variant="destructive"
                onClick={handleCancel}
                disabled={cancelMutation.isPending}
                className="w-full"
              >
                {cancelMutation.isPending ? (
                  <>
                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                    İptal Ediliyor...
                  </>
                ) : (
                  'Rezervasyonu İptal Et'
                )}
              </Button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
