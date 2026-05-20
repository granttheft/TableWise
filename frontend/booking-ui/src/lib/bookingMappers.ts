import type {
  AvailabilitySlot,
  AvailableTable,
  TableCombination,
  VenueConfig,
  VenueCustomField,
  VenueClosure,
  WorkingHour,
} from '@/types/api';

/** Backend VenueConfigDto (camelCase JSON). */
interface VenueConfigApiDto {
  venueId?: string;
  name?: string;
  slug?: string;
  description?: string;
  logoUrl?: string;
  address?: string;
  phoneNumber?: string;
  slotDurationMinutes?: number;
  depositEnabled?: boolean;
  depositAmount?: number;
  depositPerPerson?: boolean;
  workingHours?: Record<string, WorkingHoursPeriodApi> | { days?: Record<string, WorkingHoursPeriodApi> };
  customFields?: VenueCustomFieldApi[];
  closures?: VenueClosureApi[];
  minPartySize?: number;
  maxPartySize?: number;
  minAdvanceBookingDays?: number;
  maxAdvanceBookingDays?: number;
}

interface WorkingHoursPeriodApi {
  open?: string;
  close?: string;
  isClosed?: boolean;
  isOpen?: boolean;
  openTime?: string;
  closeTime?: string;
}

interface VenueCustomFieldApi {
  fieldId?: string;
  fieldKey?: string;
  name?: string;
  label?: string;
  fieldType?: string;
  isRequired?: boolean;
  options?: string[];
  placeholder?: string;
}

interface VenueClosureApi {
  id?: string;
  startDate?: string;
  endDate?: string;
  reason?: string;
}

interface AvailabilityResponseApi {
  slots?: AvailabilitySlotApi[];
  isVenueClosed?: boolean;
  closureReason?: string;
}

interface AvailabilitySlotApi {
  timeLabel?: string;
  availableTables?: TableOptionApi[];
  availableCombinations?: TableCombinationApi[];
}

interface TableOptionApi {
  tableId?: string;
  name?: string;
  capacity?: number;
  location?: string;
}

interface TableCombinationApi {
  combinationId?: string;
  name?: string;
  combinedCapacity?: number;
}

const DAY_KEY_TO_INDEX: Record<string, number> = {
  sunday: 0,
  monday: 1,
  tuesday: 2,
  wednesday: 3,
  thursday: 4,
  friday: 5,
  saturday: 6,
  '0': 0,
  '1': 1,
  '2': 2,
  '3': 3,
  '4': 4,
  '5': 5,
  '6': 6,
};

function defaultWorkingHours(): WorkingHour[] {
  return [0, 1, 2, 3, 4, 5, 6].map((dayOfWeek) => ({
    dayOfWeek,
    isOpen: true,
    openTime: '12:00',
    closeTime: '23:00',
  }));
}

function resolveDayIndex(key: string): number | undefined {
  const normalized = key.trim().toLowerCase();
  if (normalized in DAY_KEY_TO_INDEX) {
    return DAY_KEY_TO_INDEX[normalized];
  }
  const parsed = Number.parseInt(normalized, 10);
  if (!Number.isNaN(parsed) && parsed >= 0 && parsed <= 6) {
    return parsed;
  }
  return undefined;
}

function mapWorkingHoursFromApi(raw?: unknown): WorkingHour[] {
  if (!raw || typeof raw !== 'object') {
    return defaultWorkingHours();
  }

  const record = raw as Record<string, unknown>;
  let entries: Record<string, WorkingHoursPeriodApi>;
  if (record.days && typeof record.days === 'object') {
    entries = record.days as Record<string, WorkingHoursPeriodApi>;
  } else {
    entries = record as Record<string, WorkingHoursPeriodApi>;
  }

  const keys = Object.keys(entries);
  if (keys.length === 0) {
    return defaultWorkingHours();
  }

  const hours: WorkingHour[] = [];
  for (const key of keys) {
    const dayOfWeek = resolveDayIndex(key);
    if (dayOfWeek === undefined) continue;

    const period = entries[key];
    const isClosed = period.isClosed === true || period.isOpen === false;

    hours.push({
      dayOfWeek,
      isOpen: !isClosed,
      openTime: period.open ?? period.openTime,
      closeTime: period.close ?? period.closeTime,
    });
  }

  return hours.length > 0 ? hours : defaultWorkingHours();
}

function mapFieldType(apiType?: string): VenueCustomField['fieldType'] {
  const t = (apiType ?? 'text').toLowerCase();
  if (t === 'number') return 'number';
  if (t === 'select') return 'select';
  if (t === 'date') return 'date';
  if (t === 'checkbox' || t === 'boolean') return 'boolean';
  return 'text';
}

function mapCustomFields(fields?: VenueCustomFieldApi[]): VenueCustomField[] {
  if (!fields?.length) return [];

  return fields.map((f, index) => ({
    id: f.fieldId ?? f.fieldKey ?? f.name ?? String(index),
    fieldKey: f.fieldKey ?? f.name ?? '',
    fieldLabel: f.label ?? f.fieldKey ?? f.name ?? '',
    fieldType: mapFieldType(f.fieldType),
    isRequired: f.isRequired ?? false,
    selectOptions: f.options,
    displayOrder: index,
  }));
}

function mapClosures(closures?: VenueClosureApi[]): VenueClosure[] {
  if (!closures?.length) return [];

  return closures.map((c) => ({
    id: c.id ?? '',
    startDate: c.startDate ?? '',
    endDate: c.endDate ?? c.startDate ?? '',
    reason: c.reason ?? 'Kapali',
  }));
}

function mapTableOption(t: TableOptionApi): AvailableTable {
  return {
    tableId: t.tableId ?? '',
    tableName: t.name ?? '',
    capacity: t.capacity ?? 0,
    location: t.location,
  };
}

/**
 * Backend config DTO'sunu booking UI VenueConfig tipine donusturur.
 */
export function mapVenueConfigFromApi(data: VenueConfigApiDto): VenueConfig {
  const advanceDays = data.maxAdvanceBookingDays ?? 60;

  return {
    venueName: data.name ?? 'Rezervasyon',
    slug: data.slug ?? '',
    description: data.description,
    address: data.address,
    phone: data.phoneNumber,
    logoUrl: data.logoUrl,
    maxPartySize: data.maxPartySize ?? 20,
    minPartySize: data.minPartySize ?? 1,
    slotDuration: data.slotDurationMinutes ?? 90,
    advanceBookingDays: advanceDays > 0 ? advanceDays : 60,
    customFields: mapCustomFields(data.customFields),
    workingHours: mapWorkingHoursFromApi(data.workingHours),
    closures: mapClosures(data.closures),
    depositSettings: data.depositEnabled
      ? {
          requireDeposit: true,
          depositAmount: data.depositAmount,
        }
      : undefined,
  };
}

/**
 * Backend availability response'unu slot listesine donusturur.
 */
export function mapAvailabilityFromApi(data: AvailabilityResponseApi): AvailabilitySlot[] {
  if (data.isVenueClosed) {
    return [];
  }

  const slots = data.slots ?? [];
  return slots.map((slot) => {
    const tables = (slot.availableTables ?? []).map(mapTableOption);
    const combinations: TableCombination[] = (slot.availableCombinations ?? []).map(
      (c) => ({
        combinationId: c.combinationId ?? '',
        tables: [
          {
            tableId: c.combinationId ?? '',
            tableName: c.name ?? 'Kombine masa',
            capacity: c.combinedCapacity ?? 0,
          },
        ],
        totalCapacity: c.combinedCapacity ?? 0,
      })
    );

    return {
      time: slot.timeLabel ?? '',
      availableTables: tables,
      tableCombinations: combinations,
    };
  });
}
