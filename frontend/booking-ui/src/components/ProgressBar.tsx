import { cn } from '@/lib/utils';

interface ProgressBarProps {
  currentStep: number;
  totalSteps: number;
}

export function ProgressBar({ currentStep, totalSteps }: ProgressBarProps) {
  const progress = (currentStep / totalSteps) * 100;

  const steps = [
    { number: 1, label: 'Tarih' },
    { number: 2, label: 'Saat' },
    { number: 3, label: 'Masa' },
    { number: 4, label: 'Bilgiler' },
    { number: 5, label: 'Onay' },
  ];

  return (
    <div className="w-full mb-8">
      {/* Progress bar */}
      <div className="relative h-2 bg-gray-200 rounded-full overflow-hidden mb-4">
        <div
          className="absolute top-0 left-0 h-full bg-primary transition-all duration-300 ease-out"
          style={{ width: `${progress}%` }}
        />
      </div>

      {/* Step indicators */}
      <div className="flex justify-between items-center">
        {steps.map((step) => (
          <div
            key={step.number}
            className="flex flex-col items-center gap-1"
          >
            <div
              className={cn(
                'w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-colors',
                step.number < currentStep
                  ? 'bg-primary text-white'
                  : step.number === currentStep
                  ? 'bg-primary text-white ring-4 ring-primary/20'
                  : 'bg-gray-200 text-gray-500'
              )}
            >
              {step.number}
            </div>
            <span
              className={cn(
                'text-xs font-medium hidden sm:block',
                step.number === currentStep
                  ? 'text-primary'
                  : 'text-gray-500'
              )}
            >
              {step.label}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
