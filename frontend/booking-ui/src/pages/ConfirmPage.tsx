import { useParams, Link } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { motion } from 'framer-motion';
import { CheckCircle, Copy, Calendar, MapPin, Home, MessageCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { useReservationDetail } from '@/hooks/useReservationDetail';
import { toast } from 'sonner';
import { formatDate, formatTime, maskEmail } from '@/lib/utils';
import { Loader2 } from 'lucide-react';

export function ConfirmPage() {
  const { code } = useParams<{ code: string }>();
  const { data: reservation, isLoading, error } = useReservationDetail(code!);

  const handleCopyCode = () => {
    navigator.clipboard.writeText(code!);
    toast.success('Onay kodu kopyalandı');
  };

  const handleAddToCalendar = () => {
    if (!reservation) return;

    const dateTime = new Date(`${reservation.date}T${reservation.time}`);
    const endTime = new Date(dateTime.getTime() + 2 * 60 * 60 * 1000); // +2 hours

    const title = `Rezervasyon - ${reservation.venueName}`;
    const details = `${reservation.partySize} kişilik rezervasyon\nOnay Kodu: ${reservation.confirmationCode}`;

    const googleCalendarUrl = `https://calendar.google.com/calendar/render?action=TEMPLATE&text=${encodeURIComponent(
      title
    )}&dates=${dateTime.toISOString().replace(/[-:]/g, '').split('.')[0]}Z/${endTime
      .toISOString()
      .replace(/[-:]/g, '')
      .split('.')[0]}Z&details=${encodeURIComponent(details)}`;

    window.open(googleCalendarUrl, '_blank');
  };

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

  return (
    <>
      <Helmet>
        <title>Rezervasyon Onayı | Tablewise</title>
        <meta
          name="description"
          content="Rezervasyonunuz başarıyla oluşturuldu"
        />
      </Helmet>

      <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4 py-8">
        <div className="max-w-2xl w-full space-y-6">
          {/* Success animation */}
          <motion.div
            initial={{ scale: 0, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ duration: 0.5, type: 'spring' }}
            className="flex justify-center"
          >
            <div className="w-24 h-24 bg-green-100 rounded-full flex items-center justify-center">
              <CheckCircle className="w-16 h-16 text-green-600" />
            </div>
          </motion.div>

          <motion.div
            initial={{ y: 20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ delay: 0.3 }}
            className="text-center"
          >
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Rezervasyonunuz Oluşturuldu!
            </h1>
            <p className="text-muted-foreground">
              Onay e-postası{' '}
              <span className="font-medium">
                {maskEmail(reservation.customerEmail)}
              </span>{' '}
              adresine gönderildi.
            </p>
            {reservation.whatsAppConsent && (
              <div className="mt-3 inline-flex items-center gap-2 text-sm text-green-700 bg-green-50 border border-green-100 rounded-lg px-3 py-2">
                <MessageCircle className="w-4 h-4 flex-shrink-0" />
                <span>WhatsApp'tan bildirim gönderildi.</span>
              </div>
            )}
          </motion.div>

          {/* Confirmation code */}
          <Card>
            <CardContent className="pt-6">
              <div className="text-center">
                <p className="text-sm text-muted-foreground mb-2">
                  Onay Kodunuz
                </p>
                <div className="flex items-center justify-center gap-3">
                  <code className="text-3xl font-mono font-bold tracking-wider text-primary">
                    {reservation.confirmationCode}
                  </code>
                  <Button
                    variant="outline"
                    size="icon"
                    onClick={handleCopyCode}
                  >
                    <Copy className="w-4 h-4" />
                  </Button>
                </div>
                <p className="text-xs text-muted-foreground mt-2">
                  Bu kodu rezervasyonunuzu görüntülemek için kullanabilirsiniz
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Reservation details */}
          <Card>
            <CardContent className="pt-6 space-y-4">
              <div>
                <h3 className="font-semibold text-lg mb-3">
                  Rezervasyon Detayları
                </h3>
              </div>

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
                  <p className="font-medium">{formatTime(reservation.time)}</p>
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
            </CardContent>
          </Card>

          {/* Actions */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <Button variant="outline" onClick={handleAddToCalendar}>
              <Calendar className="w-4 h-4 mr-2" />
              Takvime Ekle
            </Button>

            <Link to={`/rezervasyon/goruntule/${code}`}>
              <Button variant="outline" className="w-full">
                <MapPin className="w-4 h-4 mr-2" />
                Rezervasyonu Görüntüle
              </Button>
            </Link>

            <Link to="/">
              <Button className="w-full">
                <Home className="w-4 h-4 mr-2" />
                Yeni Rezervasyon
              </Button>
            </Link>
          </div>
        </div>
      </div>
    </>
  );
}
