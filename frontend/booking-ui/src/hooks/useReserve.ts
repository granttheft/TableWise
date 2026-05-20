import { useMutation } from '@tanstack/react-query';
import { createReservation } from '@/lib/api';
import type { ReservationRequest } from '@/types/api';

export function useReserve(slug: string) {
  return useMutation({
    mutationFn: (request: ReservationRequest) =>
      createReservation(slug, request),
  });
}
