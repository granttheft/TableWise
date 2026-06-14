import { useParams, Link, useNavigate } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useReservationDetail } from '@/hooks/useReservationDetail';
import { formatDate, formatTime } from '@/lib/utils';
import { Loader2, ArrowLeft, Edit, XCircle } from 'lucide-react';
import { cn } from '@/lib/utils';

const statusLabels: Record<string, string> = {
  Pending: 'Beklemede',
  Confirmed: 'Onaylandı',
  Cancelled: 'İptal Edildi',
  Completed: 'Tamamlandı',
  NoShow: 'Gelmedi',
  Modified: 'Değiştirildi',
};

const statusColors: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Confirmed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
  Completed: 'bg-blue-100 text-blue-800',
  NoShow: 'bg-gray-100 text-gray-800',
  Modified: 'bg-purple-100 text-purple-800',
};

export function ViewPage() {
  const { code } = useParams<{ code: string }>();
  const navigate = useNavigate();
  const { data: reservation, isLoading, error } = useReservationDetail(code!);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-primary" />
      </div>
    );
  }

  if (error || !reservation) {
    return (
      <div className="min-h-screen flex items-center justify-center px-4">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-600 mb-2">Hata</h1>
          <p className="text-muted-foreground mb-4">
            Rezervasyon bulunamadı. Lütfen onay kodunu kontrol edin.
          </p>
          <Link to="/">
            <Button>Ana Sayfaya Dön</Button>
          </Link>
        </div>
      </div>
    );
  }

  const reservationDate = new Date(`${reservation.date}T${reservation.time}`);
  const now = new Date();
  const hoursUntilReservation =
    (reservationDate.getTime() - now.getTime()) / (1000 * 60 * 60);

  const canModifyOrCancel =
    reservation.canModify &&
    reservation.canCancel &&
    hoursUntilReservation > 24;

  return (
    <>
      <Helmet>
        <title>Rezervasyon Detayı | Tablewise</title>
        <meta name="description" content="Rezervasyon detaylarınızı görüntüleyin" />
      </Helmet>

      <div className="min-h-screen bg-gray-50">
        <div className="max-w-2xl mx-auto px-4 py-8">
          <Link to="/">
            <Button variant="ghost" className="mb-6">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Geri
            </Button>
          </Link>

          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <h1 className="text-2xl font-bold">Rezervasyon Detayı</h1>
              <span
                className={cn(
                  'px-3 py-1 rounded-full text-sm font-medium',
                  statusColors[reservation.status]
                )}
              >
                {statusLabels[reservation.status]}
              </span>
            </div>

            {/* Confirmation code */}
            <Card>
              <CardContent className="pt-6">
                <div className="text-center">
                  <p className="text-sm text-muted-foreground mb-2">
                    Onay Kodu
                  </p>
                  <code className="text-2xl font-mono font-bold tracking-wider text-primary">
                    {reservation.confirmationCode}
                  </code>
                </div>
              </CardContent>
            </Card>

            {/* Reservation details */}
            <Card>
              <CardHeader>
                <CardTitle>Rezervasyon Bilgileri</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
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

                {reservation.tables.length > 0 && (
                  <div>
                    <p className="text-sm text-muted-foreground">Masalar</p>
                    <p className="font-medium">
                      {reservation.tables.map((t) => t.tableName).join(', ')}
                    </p>
                  </div>
                )}

                {reservation.specialRequests && (
                  <div>
                    <p className="text-sm text-muted-foreground">Özel İstek</p>
                    <p className="font-medium">{reservation.specialRequests}</p>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Customer info */}
            <Card>
              <CardHeader>
                <CardTitle>İletişim Bilgileri</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <div>
                  <p className="text-sm text-muted-foreground">Ad Soyad</p>
                  <p className="font-medium">{reservation.customerName}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">E-posta</p>
                  <p className="font-medium">{reservation.customerEmail}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Telefon</p>
                  <p className="font-medium">{reservation.customerPhone}</p>
                </div>
              </CardContent>
            </Card>

            {/* Actions */}
            {reservation.status !== 'Cancelled' && (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <Button
                  variant="outline"
                  onClick={() =>
                    navigate(`/rezervasyon/degistir/${code}`)
                  }
                  disabled={!canModifyOrCancel}
                  title={
                    !canModifyOrCancel
                      ? 'Rezervasyonunuz 24 saatten az kaldığı için değiştirilemez'
                      : ''
                  }
                >
                  <Edit className="w-4 h-4 mr-2" />
                  Değiştir
                </Button>

                <Button
                  variant="destructive"
                  onClick={() => navigate(`/rezervasyon/iptal/${code}`)}
                  disabled={!canModifyOrCancel}
                  title={
                    !canModifyOrCancel
                      ? 'Rezervasyonunuz 24 saatten az kaldığı için iptal edilemez'
                      : ''
                  }
                >
                  <XCircle className="w-4 h-4 mr-2" />
                  İptal Et
                </Button>
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
}
