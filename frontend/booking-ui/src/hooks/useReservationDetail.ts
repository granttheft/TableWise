import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getReservationDetail,
  modifyReservation,
  cancelReservation,
} from '@/lib/api';
import type {
  ModifyReservationRequest,
  CancelReservationRequest,
} from '@/types/api';

export function useReservationDetail(code: string) {
  return useQuery({
    queryKey: ['reservation', code],
    queryFn: () => getReservationDetail(code),
    retry: 2,
  });
}

export function useModifyReservation(code: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ModifyReservationRequest) =>
      modifyReservation(code, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservation', code] });
    },
  });
}

export function useCancelReservation(code: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CancelReservationRequest) =>
      cancelReservation(code, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservation', code] });
    },
  });
}
