import { useState, useMemo } from 'react';
import { Calendar } from '@/components/ui/calendar';
import { Button } from '@/components/ui/button';
import { AlertCircle } from 'lucide-react';
import type { VenueConfig } from '@/types/api';
import { addDays, isDateInRange } from '@/lib/utils';

interface Step1DateProps {
  config: VenueConfig;
  selectedDate: Date | null;
  onDateSelect: (date: Date) => void;
  onNext: () => void;
}

export function Step1Date({
  config,
  selectedDate,
  onDateSelect,
  onNext,
}: Step1DateProps) {
  const [currentMonth, setCurrentMonth] = useState(new Date());

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const maxDate = addDays(today, config.advanceBookingDays);

  // Get closures for the current month
  const closures = config.closures ?? [];
  const workingHours = config.workingHours ?? [];

  const closuresInMonth = useMemo(() => {
    return closures.filter((closure) => {
      const start = new Date(closure.startDate);
      const end = new Date(closure.endDate);
      const monthStart = new Date(
        currentMonth.getFullYear(),
        currentMonth.getMonth(),
        1
      );
      const monthEnd = new Date(
        currentMonth.getFullYear(),
        currentMonth.getMonth() + 1,
        0
      );

      // Check if closure overlaps with current month
      return start <= monthEnd && end >= monthStart;
    });
  }, [closures, currentMonth]);

  const disabledDays = (date: Date) => {
    // Before today or after max date
    if (date < today || date > maxDate) {
      return true;
    }

    // Check if day of week is closed
    const dayOfWeek = date.getDay();
    if (workingHours.length > 0) {
      const workingHour = workingHours.find((wh) => wh.dayOfWeek === dayOfWeek);
      if (!workingHour?.isOpen) {
        return true;
      }
    }

    const isInClosure = closures.some((closure) => {
      const start = new Date(closure.startDate);
      const end = new Date(closure.endDate);
      return isDateInRange(date, start, end);
    });

    return isInClosure;
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold mb-2">Tarih Seçin</h2>
        <p className="text-sm text-muted-foreground">
          Rezervasyon yapmak istediğiniz tarihi seçin
        </p>
      </div>

      {closuresInMonth.length > 0 && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 flex items-start gap-3">
          <AlertCircle className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-medium text-yellow-900">
              Bu ay içinde kapalı günler bulunmaktadır
            </p>
            <ul className="text-sm text-yellow-800 mt-2 space-y-1">
              {closuresInMonth.map((closure) => (
                <li key={closure.id}>
                  • {new Date(closure.startDate).toLocaleDateString('tr-TR')} -{' '}
                  {new Date(closure.endDate).toLocaleDateString('tr-TR')}:{' '}
                  {closure.reason}
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}

      <div className="flex justify-center">
        <Calendar
          mode="single"
          selected={selectedDate || undefined}
          onSelect={(date) => date && onDateSelect(date)}
          disabled={disabledDays}
          month={currentMonth}
          onMonthChange={setCurrentMonth}
          className="rounded-md border"
        />
      </div>

      <Button
        onClick={onNext}
        disabled={!selectedDate}
        className="w-full"
        size="lg"
      >
        Devam Et
      </Button>
    </div>
  );
}
