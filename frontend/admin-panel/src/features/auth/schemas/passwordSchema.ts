import { z } from 'zod'

/** Ortak şifre kuralları (kayıt, davet, sıfırlama). */
export const passwordFieldSchema = z
  .string()
  .min(8, 'Şifre en az 8 karakter olmalıdır')
  .regex(/[a-z]/, 'En az bir küçük harf içermelidir')
  .regex(/[A-Z]/, 'En az bir büyük harf içermelidir')
  .regex(/[0-9]/, 'En az bir rakam içermelidir')

/**
 * Şifre + tekrar alanı için Zod şeması.
 */
export function passwordWithConfirmSchema() {
  return z
    .object({
      password: passwordFieldSchema,
      confirmPassword: z.string().min(1, 'Şifre tekrarı zorunludur'),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: 'Şifreler eşleşmiyor',
      path: ['confirmPassword'],
    })
}
