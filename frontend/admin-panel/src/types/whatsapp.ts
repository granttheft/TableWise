export interface VenueWhatsAppSettings {
  whatsAppEnabled: boolean;
  notifyReservationReceived: boolean;
  notifyReservationConfirmed: boolean;
  notifyReminder: boolean;
  notifyCancellation: boolean;
  isConnected: boolean;
}

export interface WhatsAppMessageHistoryItem {
  id: string;
  toPhone: string;
  template: WhatsAppTemplate;
  status: WhatsAppStatus;
  sentAt: string | null;
  deliveredAt: string | null;
  errorMessage: string | null;
  reservationId: string | null;
}

export type WhatsAppTemplate =
  | 'ReservationReceived'
  | 'ReservationConfirmed'
  | 'Reminder'
  | 'Cancellation';

export type WhatsAppStatus = 'Queued' | 'Sent' | 'Delivered' | 'Read' | 'Failed';

export interface WhatsAppHistoryParams {
  page?: number;
  pageSize?: number;
  status?: WhatsAppStatus;
  from?: string;
  to?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
