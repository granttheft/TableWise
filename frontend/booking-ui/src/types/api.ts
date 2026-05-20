// API Response Types

export interface VenueConfig {
  venueName: string;
  slug: string;
  description?: string;
  address?: string;
  phone?: string;
  email?: string;
  logoUrl?: string;
  coverImageUrl?: string;
  maxPartySize: number;
  minPartySize: number;
  slotDuration: number;
  advanceBookingDays: number;
  customFields: VenueCustomField[];
  workingHours: WorkingHour[];
  closures: VenueClosure[];
  depositSettings?: DepositSettings;
}

export interface VenueCustomField {
  id: string;
  fieldKey: string;
  fieldLabel: string;
  fieldType: 'text' | 'number' | 'boolean' | 'date' | 'select';
  isRequired: boolean;
  selectOptions?: string[];
  displayOrder: number;
}

export interface WorkingHour {
  dayOfWeek: number; // 0=Sunday, 6=Saturday
  isOpen: boolean;
  openTime?: string; // "HH:mm"
  closeTime?: string; // "HH:mm"
}

export interface VenueClosure {
  id: string;
  startDate: string; // ISO date
  endDate: string; // ISO date
  reason: string;
}

export interface DepositSettings {
  requireDeposit: boolean;
  depositAmount?: number;
  depositPercentage?: number;
}

export interface AvailabilitySlot {
  time: string; // "HH:mm"
  availableTables: AvailableTable[];
  tableCombinations: TableCombination[];
}

export interface AvailableTable {
  tableId: string;
  tableName: string;
  capacity: number;
  location?: string;
}

export interface TableCombination {
  combinationId: string;
  tables: AvailableTable[];
  totalCapacity: number;
}

export interface RuleEvaluationRequest {
  date: string; // ISO date
  time: string; // "HH:mm"
  partySize: number;
  tableIds?: string[];
  customFieldValues?: Record<string, string | number | boolean>;
}

export interface RuleEvaluationResult {
  actions: RuleAction[];
  canProceed: boolean;
}

export interface RuleAction {
  actionType: 'BLOCK' | 'WARN' | 'DISCOUNT' | 'DEPOSIT' | 'SUGGEST';
  message: string;
  value?: number; // for DISCOUNT (percentage) or DEPOSIT (amount)
  suggestedTableId?: string; // for SUGGEST
}

export interface ReservationRequest {
  date: string;
  time: string;
  partySize: number;
  tableIds?: string[];
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  specialRequests?: string;
  customFieldValues?: Record<string, string | number | boolean>;
  acceptsKvkk: boolean;
}

export interface ReservationResponse {
  reservationId: string;
  confirmationCode: string;
  status: ReservationStatus;
  depositRequired: boolean;
  depositAmount?: number;
}

export type ReservationStatus = 
  | 'pending' 
  | 'confirmed' 
  | 'cancelled' 
  | 'completed' 
  | 'no_show';

export interface ReservationDetail {
  reservationId: string;
  confirmationCode: string;
  status: ReservationStatus;
  venueName: string;
  date: string;
  time: string;
  partySize: number;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  specialRequests?: string;
  tables: AvailableTable[];
  customFieldValues?: Record<string, string | number | boolean>;
  createdAt: string;
  depositPaid: boolean;
  depositAmount?: number;
  discountApplied?: number;
  canModify: boolean;
  canCancel: boolean;
}

export interface ModifyReservationRequest {
  newDate?: string;
  newTime?: string;
  newPartySize?: number;
  newTableIds?: string[];
}

export interface CancelReservationRequest {
  reason?: string;
}

export interface ApiError {
  message: string;
  code?: string;
  details?: Record<string, string[]>;
}
