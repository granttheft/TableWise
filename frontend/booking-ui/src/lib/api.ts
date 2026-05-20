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

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

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
  const response = await api.get<VenueConfig>(`/api/v1/book/${slug}/config`);
  return response.data;
}

export async function getAvailability(
  slug: string,
  date: string,
  partySize: number
): Promise<AvailabilitySlot[]> {
  const response = await api.get<AvailabilitySlot[]>(
    `/api/v1/book/${slug}/availability`,
    {
      params: { date, partySize },
    }
  );
  return response.data;
}

export async function evaluateRules(
  slug: string,
  request: RuleEvaluationRequest
): Promise<RuleEvaluationResult> {
  const response = await api.post<RuleEvaluationResult>(
    `/api/v1/book/${slug}/evaluate`,
    request
  );
  return response.data;
}

export async function createReservation(
  slug: string,
  request: ReservationRequest
): Promise<ReservationResponse> {
  const response = await api.post<ReservationResponse>(
    `/api/v1/book/${slug}/reserve`,
    request
  );
  return response.data;
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
