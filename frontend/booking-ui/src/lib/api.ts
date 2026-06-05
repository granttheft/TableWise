import axios, { AxiosError } from 'axios';
import type {
  VenueConfig,
  AvailabilitySlot,
  RuleEvaluationRequest,
  RuleEvaluationResult,
  ReservationRequest,
  ReservationResponse,
  ReservationDetail,
  ModifyReservationRequest,
  CancelReservationRequest,
  ApiError,
} from '@/types/api';
import { generateIdempotencyKey } from './utils';
import {
  mapAvailabilityFromApi,
  mapEvaluateRequestToApi,
  mapEvaluateResponseFromApi,
  mapVenueConfigFromApi,
} from './bookingMappers';

/** Bos ise Vite proxy /api -> localhost:5086 (admin-panel ile ayni yerel akis). */
const API_URL = import.meta.env.VITE_API_URL ?? '';

export const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Idempotency-Key interceptor for POST requests
api.interceptors.request.use((config) => {
  if (config.method === 'post' && !config.headers['Idempotency-Key']) {
    config.headers['Idempotency-Key'] = generateIdempotencyKey();
  }
  return config;
});

// Error handler
export function handleApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ApiError>;
    return {
      message: axiosError.response?.data?.message || axiosError.message,
      code: axiosError.response?.data?.code,
      details: axiosError.response?.data?.details,
    };
  }
  return {
    message: 'Bilinmeyen bir hata oluştu',
  };
}

// API Functions

export async function getVenueConfig(slug: string): Promise<VenueConfig> {
  const response = await api.get(`/api/v1/book/${slug}/config`);
  return mapVenueConfigFromApi(response.data);
}

export async function getAvailability(
  slug: string,
  date: string,
  partySize: number
): Promise<AvailabilitySlot[]> {
  const response = await api.get(`/api/v1/book/${slug}/availability`, {
    params: { date, partySize },
  });
  return mapAvailabilityFromApi(response.data);
}

export async function evaluateRules(
  slug: string,
  request: RuleEvaluationRequest
): Promise<RuleEvaluationResult> {
  const response = await api.post(
    `/api/v1/book/${slug}/evaluate`,
    mapEvaluateRequestToApi(request)
  );
  return mapEvaluateResponseFromApi(response.data);
}

export async function createReservation(
  slug: string,
  request: ReservationRequest
): Promise<ReservationResponse> {
  // Backend field adları UI field adlarından farklı — map ediyoruz
  const body = {
    guestName: request.customerName,
    guestEmail: request.customerEmail || undefined,
    guestPhone: request.customerPhone,
    partySize: request.partySize,
    reservedFor: new Date(`${request.date}T${request.time}:00`).toISOString(),
    tableId: request.tableId || undefined,
    tableCombinationId: request.tableCombinationId || undefined,
    specialRequests: request.specialRequests || undefined,
    customFieldAnswers: request.customFieldValues
      ? Object.fromEntries(
          Object.entries(request.customFieldValues).map(([k, v]) => [k, String(v)])
        )
      : undefined,
    privacyPolicyAccepted: request.acceptsKvkk,
    whatsAppConsent: request.whatsAppConsent ?? false,
  };

  const response = await api.post<any>(`/api/v1/book/${slug}/reserve`, body);

  // Backend confirmCode döndürüyor, UI confirmationCode bekliyor
  return {
    reservationId: response.data.reservationId,
    confirmationCode: response.data.confirmCode,
    status: (response.data.status || 'confirmed').toLowerCase() as ReservationResponse['status'],
    depositRequired: response.data.depositRequired ?? false,
    depositAmount: response.data.depositAmount,
    whatsAppConsent: response.data.whatsAppConsent ?? false,
  };
}

export async function getReservationDetail(
  code: string
): Promise<ReservationDetail> {
  const response = await api.get<ReservationDetail>(
    `/api/v1/book/confirm/${code}`
  );
  return response.data;
}

export async function modifyReservation(
  code: string,
  request: ModifyReservationRequest
): Promise<ReservationDetail> {
  const response = await api.patch<ReservationDetail>(
    `/api/v1/book/confirm/${code}/modify`,
    request
  );
  return response.data;
}

export async function cancelReservation(
  code: string,
  request: CancelReservationRequest
): Promise<void> {
  await api.post(`/api/v1/book/confirm/${code}/cancel`, request);
}
