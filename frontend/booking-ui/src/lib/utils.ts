import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatDate(date: Date | string): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('tr-TR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

export function formatTime(time: string): string {
  // time is "HH:mm"
  return time;
}

export function maskEmail(email: string): string {
  const [local, domain] = email.split('@');
  if (local.length <= 2) return email;
  const masked = local[0] + '*'.repeat(local.length - 2) + local[local.length - 1];
  return `${masked}@${domain}`;
}

export function generateIdempotencyKey(): string {
  return crypto.randomUUID();
}

export function getDayName(dayOfWeek: number): string {
  const days = ['Pazar', 'Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi'];
  return days[dayOfWeek];
}

export function isDateInRange(date: Date, start: Date, end: Date): boolean {
  return date >= start && date <= end;
}

export function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

export function formatPhoneNumber(phone: string): string {
  // Format to: 05XX XXX XX XX
  const cleaned = phone.replace(/\D/g, '');
  if (cleaned.length !== 11) return phone;
  return `${cleaned.slice(0, 4)} ${cleaned.slice(4, 7)} ${cleaned.slice(7, 9)} ${cleaned.slice(9)}`;
}

export function parsePhoneNumber(phone: string): string {
  // Remove all non-digits
  return phone.replace(/\D/g, '');
}

// toISOString() UTC verir — Istanbul (UTC+3) kullanıcıları için
// gece yarısı seçilen tarih bir önceki gün görünür.
// Bu fonksiyon yerel tarihi YYYY-MM-DD formatında döndürür.
export function toLocalDateString(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}
