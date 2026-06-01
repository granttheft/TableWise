import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { DepositInfo } from '@/components/DepositInfo';
import { Calendar, Clock, Users, MapPin, Loader2 } from 'lucide-react';
import { formatDate, formatTime } from '@/lib/utils';
import type {
  VenueConfig,
  AvailabilitySlot,
  RuleEvaluationResult,
} from '@/types/api';
import type { FormData } from './Step4Info';

interface Step5ConfirmProps {
  config: VenueConfig;
  selectedDate: Date;
  selectedSlot: AvailabilitySlot;
  partySize: number;
  tableIds: string[] | null;
  formData: FormData;
  evaluationResult: RuleEvaluationResult | null;
  isSubmitting: boolean;
  onSubmit: () => void;
  onBack: () => void;
}

export function Step5Confirm({
  selectedDate,
  selectedSlot,
  partySize,
  tableIds,
  formData,
  evaluationResult,
  isSubmitting,
  onSubmit,
  onBack,
}: Step5ConfirmProps) {
  const depositAction = evaluationResult?.actions.find(
    (a) => a.actionType === 'DEPOSIT'
  );
  const discountAction = evaluationResult?.actions.find(
    (a) => a.actionType === 'DISCOUNT'
  );

  const requiresDeposit = !!depositAction?.value;
  const depositAmount = depositAction?.value || 0;

  const selectedTables = tableIds
    ? selectedSlot.availableTables.filter((t) =>
        tableIds.includes(t.tableId)
      )
    : [];

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold mb-2">Rezervasyon Özeti</h2>
        <p className="text-sm text-muted-foreground">
          Lütfen rezervasyon bilgilerinizi kontrol edin
        </p>
      </div>

      {/* Summary card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Rezervasyon Detayları</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-start gap-3">
            <Calendar className="w-5 h-5 text-muted-foreground mt-0.5" />
            <div>
              <p className="font-medium">{formatDate(selectedDate)}</p>
              <p className="text-sm text-muted-foreground">Tarih</p>
            </div>
          </div>

          <div className="flex items-start gap-3">
            <Clock className="w-5 h-5 text-muted-foreground mt-0.5" />
            <div>
              <p className="font-medium">{formatTime(selectedSlot.time)}</p>
              <p className="text-sm text-muted-foreground">Saat</p>
            </div>
          </div>

          <div className="flex items-start gap-3">
            <Users className="w-5 h-5 text-muted-foreground mt-0.5" />
            <div>
              <p className="font-medium">{partySize} Kişi</p>
              <p className="text-sm text-muted-foreground">Kişi Sayısı</p>
            </div>
          </div>

          {selectedTables.length > 0 && (
            <div className="flex items-start gap-3">
              <MapPin className="w-5 h-5 text-muted-foreground mt-0.5" />
              <div>
                <p className="font-medium">
                  {selectedTables.map((t) => t.tableName).join(', ')}
                </p>
                <p className="text-sm text-muted-foreground">Masa</p>
              </div>
            </div>
          )}

          {!tableIds && (
            <div className="flex items-start gap-3">
              <MapPin className="w-5 h-5 text-muted-foreground mt-0.5" />
              <div>
                <p className="font-medium">Sistem Otomatik Seçecek</p>
                <p className="text-sm text-muted-foreground">Masa</p>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Customer info */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">İletişim Bilgileri</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          <div>
            <p className="text-sm text-muted-foreground">Ad Soyad</p>
            <p className="font-medium">{formData.customerName}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">E-posta</p>
            <p className="font-medium">{formData.customerEmail}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Telefon</p>
            <p className="font-medium">{formData.customerPhone}</p>
          </div>
          {formData.specialRequests && (
            <div>
              <p className="text-sm text-muted-foreground">Özel İstek</p>
              <p className="font-medium">{formData.specialRequests}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Discount info */}
      {discountAction && discountAction.value && (
        <Card className="border-green-200 bg-green-50">
          <CardContent className="pt-6">
            <p className="text-green-800 font-semibold text-center">
              🎉 %{discountAction.value} İndirim Uygulanacak!
            </p>
          </CardContent>
        </Card>
      )}

      {/* Deposit info */}
      {requiresDeposit && <DepositInfo amount={depositAmount} />}

      {/* Actions */}
      <div className="flex gap-3">
        <Button
          type="button"
          variant="outline"
          onClick={onBack}
          disabled={isSubmitting}
          className="flex-1 h-11"
        >
          Geri
        </Button>
        <Button
          onClick={onSubmit}
          disabled={isSubmitting}
          className="flex-1 h-11"
          size="lg"
        >
          {isSubmitting ? (
            <>
              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              İşleniyor...
            </>
          ) : requiresDeposit ? (
            'Ödemeye Geç'
          ) : (
            'Rezervasyon Yap'
          )}
        </Button>
      </div>

      {requiresDeposit && (
        <p className="text-xs sm:text-sm text-center text-muted-foreground">
          Bir sonraki adımda güvenli ödeme sayfasına yönlendirileceksiniz
        </p>
      )}
    </div>
  );
}
