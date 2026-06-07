import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import type { InvitePlatformUserDto, PlatformUserDto, UpdatePlatformUserRoleDto } from '@/types/api'

const QUERY_KEY = ['platform-users']

export function usePlatformUsers() {
  return useQuery<PlatformUserDto[]>({
    queryKey: QUERY_KEY,
    queryFn: () => api.get<PlatformUserDto[]>('/api/platform/users').then((r) => r.data),
    staleTime: 30_000,
  })
}

export function useInvitePlatformUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (dto: InvitePlatformUserDto) =>
      api.post<PlatformUserDto>('/api/platform/users', dto).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useUpdatePlatformUserRole(userId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (dto: UpdatePlatformUserRoleDto) =>
      api.put<PlatformUserDto>(`/api/platform/users/${userId}/role`, dto).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useTogglePlatformUserActive(userId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () =>
      api.put<PlatformUserDto>(`/api/platform/users/${userId}/toggle-active`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}
