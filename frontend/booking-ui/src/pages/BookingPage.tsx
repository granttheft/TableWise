import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { toast } from 'sonner';
import { VenueHeader } from '@/components/VenueHeader';
import { ProgressBar } from '@/components/ProgressBar';
import { Step1Date } from '@/steps/Step1Date';
import { Step2TimeAndPeople } from '@/steps/Step2TimeAndPeople';
import { Step3Table } from '@/steps/Step3Table';
import { Step4Info, type FormData } from '@/steps/Step4Info';
import { Step5Confirm } from '@/steps/Step5Confirm';
import { useVenueConfig } from '@/hooks/useVenueConfig';
import { useReserve } from '@/hooks/useReserve';
import { useEvaluate } from '@/hooks/useEvaluate';
import { Loader2 } from 'lucide-react';
import type { AvailabilitySlot } from '@/types/api';
import { handleApiError } from '@/lib/api';

export function BookingPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();

  const [currentStep, setCurrentStep] = useState(1);
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [partySize, setPartySize] = useState(2);
  const [selectedSlot, setSelectedSlot] = useState<AvailabilitySlot | null>(
    null
  );
  const [selectedTableIds, setSelectedTableIds] = useState<string[] | null>(
    null
  );
  const [formData, setFormData] = useState<FormData>({
    customerName: '',
    customerEmail: '',
    customerPhone: '',
    customFieldValues: {},
    acceptsKvkk: false,
  });

  const { data: config, isLoading, error } = useVenueConfig(slug!);
  const reserveMutation = useReserve(slug!);

  // Evaluation for final step
  const dateStr = selectedDate?.toISOString().split('T')[0];
  const evaluationRequest =
    selectedDate && selectedSlot && formData.customerName
      ? {
          date: dateStr!,
          time: selectedSlot.time,
          partySize,
          tableIds: selectedTableIds || undefined,
          customFieldValues: formData.customFieldValues,
        }
      : null;

  const { data: evaluationResult } = useEvaluate(
    slug!,
    evaluationRequest,
    currentStep === 5 && !!evaluationRequest
  );

  const handleReserve = async () => {
    if (!selectedDate || !selectedSlot) return;

    const dateStr = selectedDate.toISOString().split('T')[0];

    try {
      const response = await reserveMutation.mutateAsync({
        date: dateStr,
        time: selectedSlot.time,
        partySize,
        tableIds: selectedTableIds || undefined,
        customerName: formData.customerName,
        customerEmail: formData.customerEmail,
        customerPhone: formData.customerPhone,
        specialRequests: formData.specialRequests || undefined,
        customFieldValues: formData.customFieldValues,
        acceptsKvkk: formData.acceptsKvkk,
      });

      if (response.depositRequired) {
        // TODO: Redirect to payment (Faz 7)
        toast.info('Ödeme sayfası Faz 7\'de eklenecek');
        setTimeout(() => {
          navigate(`/rezervasyon/onay/${response.confirmationCode}`);
        }, 1000);
      } else {
        navigate(`/rezervasyon/onay/${response.confirmationCode}`);
      }
    } catch (err) {
      const apiError = handleApiError(err);
      toast.error(apiError.message || 'Rezervasyon oluşturulamadı');
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-primary" />
      </div>
    );
  }

  if (error || !config) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-600 mb-2">Hata</h1>
          <p className="text-muted-foreground">
            Mekan bilgisi yüklenemedi. Lütfen daha sonra tekrar deneyin.
          </p>
        </div>
      </div>
    );
  }

  return (
    <>
      <Helmet>
        <title>{`${config.venueName} - Rezervasyon | Tablewise`}</title>
        <meta
          name="description"
          content={`${config.venueName} icin online rezervasyon yapin`}
        />
        <meta
          property="og:title"
          content={`${config.venueName} — Rezervasyon`}
        />
        <meta
          property="og:description"
          content={`${config.venueName} icin online rezervasyon yapin`}
        />
        {config.coverImageUrl && (
          <meta property="og:image" content={config.coverImageUrl} />
        )}
      </Helmet>

      <div className="min-h-screen bg-gray-50">
        <VenueHeader config={config} />

        <div className="max-w-2xl mx-auto px-4 py-8">
          <ProgressBar currentStep={currentStep} totalSteps={5} />

          <div className="bg-white rounded-lg shadow-sm border p-6">
            {currentStep === 1 && (
              <Step1Date
                config={config}
                selectedDate={selectedDate}
                onDateSelect={setSelectedDate}
                onNext={() => setCurrentStep(2)}
              />
            )}

            {currentStep === 2 && selectedDate && (
              <Step2TimeAndPeople
                slug={slug!}
                selectedDate={selectedDate}
                partySize={partySize}
                onPartySizeChange={setPartySize}
                selectedSlot={selectedSlot}
                onSlotSelect={setSelectedSlot}
                onNext={() => setCurrentStep(3)}
                onBack={() => setCurrentStep(1)}
              />
            )}

            {currentStep === 3 && selectedSlot && (
              <Step3Table
                slot={selectedSlot}
                selectedTableIds={selectedTableIds}
                onTableSelect={setSelectedTableIds}
                onNext={() => setCurrentStep(4)}
                onBack={() => setCurrentStep(2)}
              />
            )}

            {currentStep === 4 && selectedDate && selectedSlot && (
              <Step4Info
                config={config}
                selectedDate={selectedDate}
                selectedSlot={selectedSlot}
                partySize={partySize}
                tableIds={selectedTableIds}
                formData={formData}
                onFormDataChange={setFormData}
                onNext={() => setCurrentStep(5)}
                onBack={() => setCurrentStep(3)}
              />
            )}

            {currentStep === 5 && selectedDate && selectedSlot && (
              <Step5Confirm
                config={config}
                selectedDate={selectedDate}
                selectedSlot={selectedSlot}
                partySize={partySize}
                tableIds={selectedTableIds}
                formData={formData}
                evaluationResult={evaluationResult || null}
                isSubmitting={reserveMutation.isPending}
                onSubmit={handleReserve}
                onBack={() => setCurrentStep(4)}
              />
            )}
          </div>
        </div>
      </div>
    </>
  );
}
