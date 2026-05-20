import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/api'
import { toast } from 'sonner'
import { getApiErrorMessage } from '@/lib/mapAuthResult'
import type { StaffMember, StaffInvite } from '@/types/api'

function staffApiErrorMessage(error: unknown, fallback: string): string {
  return getApiErrorMessage(error, fallback)
}

const staffBase = '/api/v1/staff'

/** API StaffMemberDto → UI model. */
interface StaffMemberApiDto {
  id: string
  email: string
  firstName: string
  lastName: string
  role: string | number
  isActive: boolean
  lastLoginAt?: string | null
  createdAt: string
}

/** API InvitationDto → UI model. */
interface InvitationApiDto {
  id: string
  email: string
  role: string | number
  invitedAt: string
  expiresAt: string
  isAccepted: boolean
  isPending: boolean
}

function mapUserRole(role: string | number): 'Owner' | 'Staff' {
  if (role === 'Owner' || role === 1) return 'Owner'
  return 'Staff'
}

function mapStaffMember(dto: StaffMemberApiDto): StaffMember {
  return {
    id: dto.id,
    tenantId: '',
    fullName: [dto.firstName, dto.lastName].filter(Boolean).join(' ').trim() || dto.email,
    email: dto.email,
    role: mapUserRole(dto.role),
    isActive: dto.isActive,
    lastLoginAt: dto.lastLoginAt ?? undefined,
    createdAt: dto.createdAt,
    updatedAt: dto.createdAt,
  }
}

function mapStaffInvite(dto: InvitationApiDto): StaffInvite {
  return {
    id: dto.id,
    tenantId: '',
    email: dto.email,
    role: mapUserRole(dto.role),
    token: '',
    expiresAt: dto.expiresAt,
    createdAt: dto.invitedAt,
  }
}

/**
 * Aktif ekip üyelerini listeler.
 */
export function useStaffMembers(activeOnly = true) {
  return useQuery({
    queryKey: ['staff', { activeOnly }],
    queryFn: async () => {
      const response = await api.get<StaffMemberApiDto[]>(staffBase, {
        params: { activeOnly },
      })
      return response.data.map(mapStaffMember)
    },
  })
}

/**
 * Davet listesini getirir (varsayılan: bekleyenler).
 */
export function useStaffInvites(pendingOnly = true) {
  return useQuery({
    queryKey: ['staff-invites', { pendingOnly }],
    queryFn: async () => {
      const response = await api.get<InvitationApiDto[]>(`${staffBase}/invitations`, {
        params: { pendingOnly },
      })
      return response.data.filter((i) => i.isPending).map(mapStaffInvite)
    },
  })
}

/**
 * Yeni personel daveti gönderir.
 */
export function useInviteStaff() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { email: string; role: 'Owner' | 'Staff'; message?: string }) => {
      const response = await api.post<string>(`${staffBase}/invitations`, data)
      return response.data
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['staff-invites'] })
      toast.success('Davet gönderildi')
    },
    onError: (error: unknown) => {
      toast.error(staffApiErrorMessage(error, 'Davet gönderilemedi'))
    },
  })
}

/**
 * Daveti yeniden gönderir.
 */
export function useResendInvite() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (inviteId: string) => {
      await api.post(`${staffBase}/invitations/${inviteId}/resend`)
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['staff-invites'] })
      toast.success('Davet tekrar gönderildi')
    },
    onError: (error: unknown) => {
      toast.error(staffApiErrorMessage(error, 'Davet tekrar gönderilemedi'))
    },
  })
}

/**
 * Bekleyen daveti iptal eder.
 */
export function useCancelInvite() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (inviteId: string) => {
      await api.delete(`${staffBase}/invitations/${inviteId}`)
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['staff-invites'] })
      toast.success('Davet iptal edildi')
    },
    onError: (error: unknown) => {
      toast.error(staffApiErrorMessage(error, 'Davet iptal edilemedi'))
    },
  })
}

/**
 * Personel rolünü günceller.
 */
export function useChangeStaffRole() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: { userId: string; role: 'Owner' | 'Staff' }) => {
      await api.put(`${staffBase}/${data.userId}/role`, { role: data.role })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['staff'] })
      toast.success('Rol güncellendi')
    },
    onError: (error: unknown) => {
      toast.error(staffApiErrorMessage(error, 'Rol güncellenemedi'))
    },
  })
}

/**
 * Personeli kaldırır (soft delete).
 */
export function useRemoveStaff() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (userId: string) => {
      await api.delete(`${staffBase}/${userId}`)
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['staff'] })
      toast.success('Kullanıcı kaldırıldı')
    },
    onError: (error: unknown) => {
      toast.error(staffApiErrorMessage(error, 'Kullanıcı kaldırılamadı'))
    },
  })
}
