import { useCurrentTenant } from '@/hooks/useCurrentTenant'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Calendar } from 'lucide-react'
import { differenceInDays } from 'date-fns'

export function TrialBanner() {
  const tenant = useCurrentTenant()

  if (!tenant || tenant.planStatus !== 'TrialActive' || !tenant.trialEndsAt) {
    return null
  }

  const daysLeft = differenceInDays(new Date(tenant.trialEndsAt), new Date())

  // Trial bitmiş gösterme
  if (daysLeft < 0) {
    return null
  }

  return (
    <div className="border-b border-amber-500/20 bg-amber-500/10 px-4 py-3">
      <Alert variant="default" className="border-amber-500/50 bg-transparent">
        <Calendar className="h-4 w-4 text-amber-500" />
        <AlertDescription className="flex items-center justify-between">
          <span className="text-sm text-amber-500">
            <strong>Deneme süreniz {daysLeft} gün sonra bitiyor.</strong> Kesintisiz hizmetten yararlanmak için planınızı yükseltin.
          </span>
          <Button
            size="sm"
            className="ml-4 bg-amber-500 text-white hover:bg-amber-600"
            onClick={() => (window.location.href = '/subscription')}
          >
            Planı Yükselt
          </Button>
        </AlertDescription>
      </Alert>
    </div>
  )
}
