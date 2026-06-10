export const ADMIN_PANEL_URL =
  import.meta.env.VITE_ADMIN_PANEL_URL ?? 'http://localhost:3000'

export const BOOKING_UI_URL =
  import.meta.env.VITE_BOOKING_UI_URL ?? 'http://localhost:5174'

export const FALLBACK_PLANS = [
  { planName: 'Starter',    monthlyPrice: 1490, yearlyPrice: 1192 },
  { planName: 'Pro',        monthlyPrice: 2990, yearlyPrice: 2392 },
  { planName: 'Business',   monthlyPrice: 5490, yearlyPrice: 4392 },
  { planName: 'Enterprise', monthlyPrice: 0,    yearlyPrice: 0    },
]
