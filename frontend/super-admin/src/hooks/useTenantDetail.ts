import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { TenantDetailDto } from '@/types/api'

export function useTenantDetail(id: string) {
  return useQuery<TenantDetailDto>({
    queryKey: ['tenant-detail', id],
    queryFn: () =>
      api.get<TenantDetailDto>(`/api/platform/tenants/${id}`).then((r) => r.data),
    enabled: !!id,
    staleTime: 30_000,
  })
}
