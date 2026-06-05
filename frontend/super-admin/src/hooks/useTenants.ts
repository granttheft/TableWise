import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'
import type { PagedResult, PlanStatus, TenantSummaryDto } from '@/types/api'

interface TenantsParams {
  page: number
  pageSize: number
  search?: string
  status?: PlanStatus | ''
}

export function useTenants(params: TenantsParams) {
  return useQuery<PagedResult<TenantSummaryDto>>({
    queryKey: ['tenants', params],
    queryFn: () =>
      api
        .get<PagedResult<TenantSummaryDto>>('/api/platform/tenants', {
          params: {
            page: params.page,
            pageSize: params.pageSize,
            search: params.search || undefined,
            status: params.status || undefined,
          },
        })
        .then((r) => r.data),
    staleTime: 30_000,
  })
}
