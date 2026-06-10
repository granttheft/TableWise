import { useState, useEffect } from 'react'
import { FALLBACK_PLANS } from '@/config'

interface ApiPlan {
  id: string
  name: string
  tier: string
  monthlyPriceTry: number
  yearlyPriceTry: number
  isVisible: boolean
  limitsJson: string
  featuresJson: string
}

export interface PlanLimits {
  maxVenues: number
  maxTables: number
  maxRules: number
  maxReservationsPerMonth: number
  maxStaffAccounts: number
}

export interface PlanFeatures {
  depositEnabled: boolean
  signalREnabled: boolean
  apiEnabled: boolean
  customWhatsAppEnabled: boolean
  analyticsEnabled: boolean
}

export interface NormalizedPlan {
  id: string
  planName: string
  monthlyPrice: number
  yearlyPrice: number
  limits: PlanLimits
  features: PlanFeatures
}

const DEFAULT_LIMITS: PlanLimits = {
  maxVenues: 1, maxTables: 50, maxRules: 5, maxReservationsPerMonth: 200, maxStaffAccounts: 3,
}
const DEFAULT_FEATURES: PlanFeatures = {
  depositEnabled: false, signalREnabled: true, apiEnabled: false,
  customWhatsAppEnabled: false, analyticsEnabled: false,
}

function parseLimits(json: string): PlanLimits {
  try { return { ...DEFAULT_LIMITS, ...JSON.parse(json) } } catch { return DEFAULT_LIMITS }
}

function parseFeatures(json: string): PlanFeatures {
  try { return { ...DEFAULT_FEATURES, ...JSON.parse(json) } } catch { return DEFAULT_FEATURES }
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
            limits:   parseLimits(p.limitsJson),
            features: parseFeatures(p.featuresJson),
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
    ? FALLBACK_PLANS.map((p, i) => ({ id: String(i), ...p }))
    : plans

  return { plans: effectivePlans, loading, error }
}
