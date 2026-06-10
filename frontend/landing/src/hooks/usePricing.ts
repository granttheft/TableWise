import { useState, useEffect } from 'react'
import { FALLBACK_PLANS } from '@/config'

interface ApiPlan {
  id: string
  name: string
  tier: string
  monthlyPriceTry: number
  yearlyPriceTry: number
  isVisible: boolean
}

export interface NormalizedPlan {
  id: string
  planName: string
  monthlyPrice: number
  yearlyPrice: number
}

export function usePricing() {
  const [plans, setPlans] = useState<NormalizedPlan[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(false)

  useEffect(() => {
    fetch('/api/public/pricing')
      .then(r => {
        if (!r.ok) throw new Error('API error')
        return r.json() as Promise<ApiPlan[]>
      })
      .then(data => {
        const normalized = data
          .filter(p => p.isVisible)
          .map(p => ({
            id: p.id,
            planName: p.name,
            monthlyPrice: p.monthlyPriceTry,
            yearlyPrice: p.yearlyPriceTry,
          }))
        setPlans(normalized)
        setLoading(false)
      })
      .catch(() => {
        setError(true)
        setLoading(false)
      })
  }, [])

  const effectivePlans: NormalizedPlan[] = error || (!loading && plans.length === 0)
    ? FALLBACK_PLANS.map((p, i) => ({ id: String(i), planName: p.planName, monthlyPrice: p.monthlyPrice, yearlyPrice: p.yearlyPrice }))
    : plans

  return { plans: effectivePlans, loading, error }
}
