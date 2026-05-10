import { useAuthStore } from '@/stores/authStore'

export function useCurrentTenant() {
  const user = useAuthStore((state) => state.user)

  if (!user) {
    return null
  }

  return {
    id: user.tenantId,
    name: user.tenantName,
    planName: user.planName,
    planStatus: user.planStatus,
    isTrialExpired: user.planStatus === 'TrialExpired',
    isSuspended: user.planStatus === 'Suspended',
    trialEndsAt: user.trialEndsAt,
  }
}
