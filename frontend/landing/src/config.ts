export const ADMIN_PANEL_URL =
  import.meta.env.VITE_ADMIN_PANEL_URL ?? 'http://localhost:3000'

export const BOOKING_UI_URL =
  import.meta.env.VITE_BOOKING_UI_URL ?? 'http://localhost:5174'

export const FALLBACK_PLANS = [
  {
    planName: 'Starter',
    monthlyPrice: 1490,
    yearlyPrice: 1192,
    limits: { maxVenues: 1, maxTables: 50, maxRules: 5, maxReservationsPerMonth: 200, maxStaffAccounts: 3 },
    features: { depositEnabled: false, signalREnabled: false, apiEnabled: false, customWhatsAppEnabled: false, analyticsEnabled: false },
  },
  {
    planName: 'Pro',
    monthlyPrice: 2990,
    yearlyPrice: 2392,
    limits: { maxVenues: 3, maxTables: -1, maxRules: -1, maxReservationsPerMonth: -1, maxStaffAccounts: 10 },
    features: { depositEnabled: true, signalREnabled: true, apiEnabled: false, customWhatsAppEnabled: false, analyticsEnabled: true },
  },
  {
    planName: 'Business',
    monthlyPrice: 5490,
    yearlyPrice: 4392,
    limits: { maxVenues: -1, maxTables: -1, maxRules: -1, maxReservationsPerMonth: -1, maxStaffAccounts: -1 },
    features: { depositEnabled: true, signalREnabled: true, apiEnabled: true, customWhatsAppEnabled: false, analyticsEnabled: true },
  },
  {
    planName: 'Enterprise',
    monthlyPrice: 0,
    yearlyPrice: 0,
    limits: { maxVenues: -1, maxTables: -1, maxRules: -1, maxReservationsPerMonth: -1, maxStaffAccounts: -1 },
    features: { depositEnabled: true, signalREnabled: true, apiEnabled: true, customWhatsAppEnabled: true, analyticsEnabled: true },
  },
]
