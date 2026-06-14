import { AlertCircle, AlertTriangle, CheckCircle, Info } from 'lucide-react';
import type { RuleAction } from '@/types/api';
import { cn } from '@/lib/utils';

interface RuleResultBannerProps {
  actions: RuleAction[];
}

export function RuleResultBanner({ actions }: RuleResultBannerProps) {
  if (actions.length === 0) return null;

  return (
    <div className="space-y-2">
      {actions.map((action, index) => {
        const isBlock = action.actionType === 'Block';
        const isWarn = action.actionType === 'Warn';
        const isDiscount = action.actionType === 'Discount';
        const isDeposit = action.actionType === 'Deposit';
        // Redirect henüz tam implement edilmedi — Suggest gibi bilgi banner'ı olarak ele al
        const isSuggest =
          action.actionType === 'Suggest' || action.actionType === 'Redirect';

        return (
          <div
            key={index}
            className={cn(
              'flex items-start gap-3 p-4 rounded-lg border',
              isBlock && 'bg-red-50 border-red-200',
              isWarn && 'bg-yellow-50 border-yellow-200',
              isDiscount && 'bg-green-50 border-green-200',
              (isDeposit || isSuggest) && 'bg-blue-50 border-blue-200'
            )}
          >
            <div className="flex-shrink-0 mt-0.5">
              {isBlock && (
                <AlertCircle className="w-5 h-5 text-red-600" />
              )}
              {isWarn && (
                <AlertTriangle className="w-5 h-5 text-yellow-600" />
              )}
              {isDiscount && (
                <CheckCircle className="w-5 h-5 text-green-600" />
              )}
              {(isDeposit || isSuggest) && (
                <Info className="w-5 h-5 text-blue-600" />
              )}
            </div>
            <div className="flex-1">
              <p
                className={cn(
                  'text-sm font-medium',
                  isBlock && 'text-red-800',
                  isWarn && 'text-yellow-800',
                  isDiscount && 'text-green-800',
                  (isDeposit || isSuggest) && 'text-blue-800'
                )}
              >
                {action.message}
              </p>
              {isDiscount && action.value && (
                <p className="text-sm text-green-700 mt-1 font-semibold">
                  %{action.value} indirim uygulanacak!
                </p>
              )}
              {isDeposit && action.value && (
                <p className="text-sm text-blue-700 mt-1 font-semibold">
                  Ön ödeme tutarı: ₺{action.value}
                </p>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
