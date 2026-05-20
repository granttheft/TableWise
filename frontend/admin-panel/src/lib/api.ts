import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/authStore'
import { toast } from 'sonner'

// Generate UUID v4
function generateUUID(): string {
  return crypto.randomUUID()
}

const api = axios.create({
  // Development: VITE_API_URL boşsa Vite proxy üzerinden relative path (/api/*→5086) kullanılır
  baseURL: import.meta.env.VITE_API_URL ?? '',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor - Bearer token ekle
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = useAuthStore.getState().accessToken

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }

    // POST isteklerinde Idempotency-Key ekle
    if (config.method === 'post' && config.headers) {
      config.headers['Idempotency-Key'] = generateUUID()
    }

    return config
  },
  (error) => Promise.reject(error)
)

// Response interceptor - Hata yönetimi ve refresh token
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // 401 - Token geçersiz veya süresi dolmuş
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        const refreshToken = useAuthStore.getState().refreshToken
        
        if (!refreshToken) {
          throw new Error('No refresh token')
        }

        const baseUrl = import.meta.env.VITE_API_URL ?? ''
        const response = await axios.post(`${baseUrl}/api/v1/auth/refresh`, {
          refreshToken,
        })

        const body = response.data as {
          tokens?: { accessToken: string; refreshToken: string }
          accessToken?: string
          refreshToken?: string
        }
        const accessToken = body.tokens?.accessToken ?? body.accessToken
        const newRefreshToken = body.tokens?.refreshToken ?? body.refreshToken
        if (!accessToken || !newRefreshToken) {
          throw new Error('Invalid refresh response')
        }

        useAuthStore.getState().setTokens(accessToken, newRefreshToken)

        // Retry original request
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`
        }
        
        return api(originalRequest)
      } catch (refreshError) {
        // Refresh başarısız - logout yap
        useAuthStore.getState().logout()
        window.location.href = '/login'
        return Promise.reject(refreshError)
      }
    }

    // 403 - Plan limit aşımı kontrolü
    if (error.response?.status === 403) {
      const errorData = error.response.data as { errorCode?: string; message?: string }
      
      if (errorData.errorCode === 'PLAN_LIMIT_EXCEEDED') {
        toast.error('Plan limitinize ulaştınız', {
          description: errorData.message || 'Lütfen planınızı yükseltin',
          action: {
            label: 'Yükselt',
            onClick: () => window.location.href = '/subscription',
          },
        })
      }
    }

    // 5xx - Sunucu hatası
    if (error.response && error.response.status >= 500) {
      toast.error('Sistem hatası', {
        description: 'Bir sorun oluştu. Lütfen daha sonra tekrar deneyin.',
      })
    }

    // Network error
    if (!error.response) {
      toast.error('Bağlantı hatası', {
        description: 'İnternet bağlantınızı kontrol edin.',
      })
    }

    return Promise.reject(error)
  }
)

export default api
