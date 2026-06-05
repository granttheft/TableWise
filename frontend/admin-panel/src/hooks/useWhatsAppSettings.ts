import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import {
  getWhatsAppMessages,
  getWhatsAppSettings,
  sendWhatsAppTest,
  updateWhatsAppSettings,
} from '@/lib/api'
import type {
  VenueWhatsAppSettings,
  WhatsAppHistoryParams,
} from '@/types/whatsapp'

const WHATSAPP_SETTINGS_KEY = (venueId: string) => ['whatsapp-settings', venueId]
const WHATSAPP_MESSAGES_KEY = (params: WhatsAppHistoryParams) => ['whatsapp-messages', params]

export function useWhatsAppSettings(venueId: string | undefined) {
  return useQuery({
    queryKey: WHATSAPP_SETTINGS_KEY(venueId ?? ''),
    queryFn: () => getWhatsAppSettings(venueId!).then((r) => r.data),
    enabled: !!venueId,
    staleTime: 30_000,
  })
}

export function useUpdateWhatsAppSettings(venueId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: Omit<VenueWhatsAppSettings, 'isConnected'>) =>
      updateWhatsAppSettings(venueId!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: WHATSAPP_SETTINGS_KEY(venueId ?? '') })
      toast.success('WhatsApp ayarları kaydedildi.')
    },
    onError: () => {
      toast.error('Ayarlar kaydedilemedi.')
    },
  })
}

export function useSendTestMessage(venueId: string | undefined) {
  return useMutation({
    mutationFn: (toPhone: string) => sendWhatsAppTest(venueId!, toPhone),
    onSuccess: () => {
      toast.success('Test mesajı gönderildi!', {
        description: 'Birkaç saniye içinde WhatsApp mesajınız gelecek.',
      })
    },
    onError: () => {
      toast.error('Test mesajı gönderilemedi.')
    },
  })
}

export function useWhatsAppMessageHistory(params: WhatsAppHistoryParams) {
  return useQuery({
    queryKey: WHATSAPP_MESSAGES_KEY(params),
    queryFn: () => getWhatsAppMessages(params).then((r) => r.data),
    staleTime: 15_000,
  })
}
