import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/authStore'
import { toast } from 'sonner'

const api = axios.create({
  baseURL: import.meta.env.VITE_PLATFORM_API_URL ?? '',
  headers: {
    'Content-Type': 'application/json',
  },
})

api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = useAuthStore.getState().accessToken

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  },
  (error) => Promise.reject(error)
)

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout()
      window.location.href = '/login'
      return Promise.reject(error)
    }

    if (error.response && error.response.status >= 500) {
      toast.error('Sistem hatası', {
        description: 'Bir sorun oluştu. Lütfen daha sonra tekrar deneyin.',
      })
    }

    if (!error.response) {
      toast.error('Bağlantı hatası', {
        description: 'İnternet bağlantınızı kontrol edin.',
      })
    }

    return Promise.reject(error)
  }
)

export default api
