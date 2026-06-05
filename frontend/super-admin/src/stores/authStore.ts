import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { PlatformRole, PlatformUser } from '@/types/api'

interface AuthState {
  user: PlatformUser | null
  accessToken: string | null
  isAuthenticated: boolean

  login: (user: PlatformUser, accessToken: string) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      isAuthenticated: false,

      login: (user, accessToken) =>
        set({ user, accessToken, isAuthenticated: true }),

      logout: () =>
        set({ user: null, accessToken: null, isAuthenticated: false }),
    }),
    {
      name: 'tablewise-platform-auth',
    }
  )
)

export function getPlatformRole(): PlatformRole | null {
  return useAuthStore.getState().user?.role ?? null
}
