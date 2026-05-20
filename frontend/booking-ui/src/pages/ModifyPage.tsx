import { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Calendar } from '@/components/ui/calendar';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  useReservationDetail,
  useModifyReservation,
} from '@/hooks/useReservationDetail';
import { useAvailability } from '@/hooks/useAvailability';
import { formatDate, formatTime, addDays } from '@/lib/utils';
import { Loader2, ArrowLeft, Calendar as CalendarIcon, Clock } from 'lucide-react';
import { toast } from 'sonner';
import { handleApiError } from '@/lib/api';
import { cn } from '@/lib/utils';

export function ModifyPage() {
  const { code } = useParams<{ code: string }>();
  const navigate = useNavigate();

  const { data: reservation, isLoading } = useReservationDetail(code!);
  const modifyMutation = useModifyReservation(code!);

  const [isDateModalOpen, setIsDateModalOpen] = useState(false);
  const [isTimeModalOpen, setIsTimeModalOpen] = useState(false);

  const [newDate, setNewDate] = useState<Date | null>(null);
  const [newTime, setNewTime] = useState<string | null>(null);

  const dateStr = newDate?.toISOString().split('T')[0];
  const { data: slots, isLoading: slotsLoading } = useAvailability(
    'dummy-slug', // We'll need to get this from somewhere
    dateStr || null,
    reservation?.partySize || null
  );

  const handleModify = async () => {
    if (!newDate && !newTime) {
      toast.error('Lütfen değiştirmek istediğiniz bilgileri seçin');
      return;
    }

    try {
      await modifyMutation.mutateAsync({
        newDate: newDate?.toISOString().split('T')[0],
        newTime: newTime || undefined,
      });

      toast.success('Rezervasyonunuz güncellendi');
      navigate(`/rezervasyon/goruntule/${code}`);
    } catch (err) {
      const apiError = handleApiError(err);
      toast.error(apiError.message || 'Güncelleme başarısız');
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

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const maxDate = addDays(today, 60);

  return (
    <>
      <Helmet>
        <title>Rezervasyon Değiştir | Tablewise</title>
        <meta name="description" content="Rezervasyonunuzu değiştirin" />
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
              <h1 className="text-2xl font-bold mb-2">Rezervasyon Değiştir</h1>
              <p className="text-muted-foreground">
                Tarih veya saat bilgilerinizi güncelleyin
              </p>
            </div>

            {/* Current reservation */}
            <Card>
              <CardHeader>
                <CardTitle>Mevcut Rezervasyon</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <div className="grid grid-cols-2 gap-4">
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

            {/* Modify options */}
            <Card>
              <CardHeader>
                <CardTitle>Yeni Bilgiler</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <Button
                    variant="outline"
                    className="w-full justify-start"
                    onClick={() => setIsDateModalOpen(true)}
                  >
                    <CalendarIcon className="w-4 h-4 mr-2" />
                    {newDate ? formatDate(newDate) : 'Yeni Tarih Seç'}
                  </Button>
                </div>

                {newDate && (
                  <div>
                    <Button
                      variant="outline"
                      className="w-full justify-start"
                      onClick={() => setIsTimeModalOpen(true)}
                      disabled={!slots || slots.length === 0}
                    >
                      <Clock className="w-4 h-4 mr-2" />
                      {newTime ? formatTime(newTime) : 'Yeni Saat Seç'}
                    </Button>
                  </div>
                )}
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
                onClick={handleModify}
                disabled={
                  modifyMutation.isPending || (!newDate && !newTime)
                }
                className="w-full"
              >
                {modifyMutation.isPending ? (
                  <>
                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                    Güncelleniyor...
                  </>
                ) : (
                  'Rezervasyonu Güncelle'
                )}
              </Button>
            </div>
          </div>
        </div>
      </div>

      {/* Date picker modal */}
      <Dialog open={isDateModalOpen} onOpenChange={setIsDateModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Yeni Tarih Seçin</DialogTitle>
          </DialogHeader>
          <div className="flex justify-center py-4">
            <Calendar
              mode="single"
              selected={newDate || undefined}
              onSelect={(date) => {
                setNewDate(date || null);
                setNewTime(null); // Reset time when date changes
                setIsDateModalOpen(false);
              }}
              disabled={(date) => date < today || date > maxDate}
              className="rounded-md border"
            />
          </div>
        </DialogContent>
      </Dialog>

      {/* Time picker modal */}
      <Dialog open={isTimeModalOpen} onOpenChange={setIsTimeModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Yeni Saat Seçin</DialogTitle>
          </DialogHeader>
          <div className="py-4">
            {slotsLoading ? (
              <div className="flex justify-center py-8">
                <Loader2 className="w-6 h-6 animate-spin text-primary" />
              </div>
            ) : !slots || slots.length === 0 ? (
              <p className="text-center text-muted-foreground py-8">
                Bu tarih için müsait saat bulunmamaktadır.
              </p>
            ) : (
              <div className="grid grid-cols-3 gap-2 max-h-96 overflow-y-auto">
                {slots.map((slot) => (
                  <button
                    key={slot.time}
                    onClick={() => {
                      setNewTime(slot.time);
                      setIsTimeModalOpen(false);
                    }}
                    className={cn(
                      'p-3 rounded-lg border-2 font-medium transition-all',
                      newTime === slot.time
                        ? 'border-primary bg-primary text-white'
                        : 'border-gray-200 hover:border-primary/50'
                    )}
                  >
                    {slot.time}
                  </button>
                ))}
              </div>
            )}
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
