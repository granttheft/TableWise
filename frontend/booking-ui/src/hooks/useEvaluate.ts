import { useQuery } from '@tanstack/react-query';
import { evaluateRules } from '@/lib/api';
import type { RuleEvaluationRequest } from '@/types/api';

export function useEvaluate(
  slug: string,
  request: RuleEvaluationRequest | null,
  enabled: boolean = true
) {
  return useQuery({
    queryKey: ['evaluate', slug, request],
    queryFn: () => evaluateRules(slug, request!),
    enabled: enabled && !!request,
    staleTime: 30 * 1000, // 30 seconds
    retry: 1,
  });
}
