import { Button } from '@/components/ui/button';
import { Minus, Plus, Loader2 } from 'lucide-react';
import { useAvailability } from '@/hooks/useAvailability';
import { cn, formatDate } from '@/lib/utils';
import type { AvailabilitySlot } from '@/types/api';

interface Step2TimeAndPeopleProps {
  slug: string;
  selectedDate: Date;
  partySize: number;
  onPartySizeChange: (size: number) => void;
  selectedSlot: AvailabilitySlot | null;
  onSlotSelect: (slot: AvailabilitySlot) => void;
  onNext: () => void;
  onBack: () => void;
}

export function Step2TimeAndPeople({
  slug,
  selectedDate,
  partySize,
  onPartySizeChange,
  selectedSlot,
  onSlotSelect,
  onNext,
  onBack,
}: Step2TimeAndPeopleProps) {
  const dateStr = selectedDate.toISOString().split('T')[0];

  const { data: slots, isLoading } = useAvailability(
    slug,
    dateStr,
    partySize > 0 ? partySize : null
  );

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold mb-2">Kişi Sayısı ve Saat</h2>
        <p className="text-sm text-muted-foreground">
          {formatDate(selectedDate)} tarihinde kaç kişi için rezervasyon
          yapmak istiyorsunuz?
        </p>
      </div>

      {/* Party size selector */}
      <div className="flex items-center justify-center gap-4">
        <Button
          variant="outline"
          size="icon"
          className="h-12 w-12"
          onClick={() => onPartySizeChange(Math.max(1, partySize - 1))}
          disabled={partySize <= 1}
        >
          <Minus className="h-6 w-6" />
        </Button>

        <div className="text-center min-w-[100px]">
          <div className="text-4xl font-bold">{partySize}</div>
          <div className="text-sm text-muted-foreground">Kişi</div>
        </div>

        <Button
          variant="outline"
          size="icon"
          className="h-12 w-12"
          onClick={() => onPartySizeChange(Math.min(20, partySize + 1))}
          disabled={partySize >= 20}
        >
          <Plus className="h-6 w-6" />
        </Button>
      </div>

      {/* Time slots */}
      <div>
        <h3 className="font-semibold mb-3">Müsait Saatler</h3>

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="w-6 h-6 animate-spin text-primary" />
          </div>
        ) : !slots || slots.length === 0 ? (
          <div className="text-center py-12 bg-gray-50 rounded-lg border border-dashed">
            <p className="text-muted-foreground mb-4">
              Bu tarih ve kişi sayısı için müsait saat bulunmamaktadır.
            </p>
            <Button variant="outline" onClick={onBack} className="w-full sm:w-auto">
              Farklı Tarih Seç
            </Button>
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-2">
            {slots.map((slot) => (
              <button
                key={slot.time}
                onClick={() => onSlotSelect(slot)}
                className={cn(
                  'p-3 min-h-[44px] rounded-lg border-2 font-medium transition-all hover:scale-105',
                  selectedSlot?.time === slot.time
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

      <div className="flex gap-3">
        <Button variant="outline" onClick={onBack} className="flex-1">
          Geri
        </Button>
        <Button
          onClick={onNext}
          disabled={!selectedSlot}
          className="flex-1"
          size="lg"
        >
          Devam Et
        </Button>
      </div>
    </div>
  );
}
