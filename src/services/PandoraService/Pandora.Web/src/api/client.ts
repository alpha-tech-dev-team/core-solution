import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios'
import { HTTP_HEADER, HTTP_STATUS, MIME_TYPE } from '@shared/api/http'
import { useAuthStore } from '@/stores/auth'
import { API_BASE_URL, AUTH_ENDPOINTS, REQUEST_TIMEOUT_MS, RETRY_FLAG } from '@/constants/api'

type RetriableRequestConfig = AxiosRequestConfig & { [RETRY_FLAG]?: boolean }

const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: REQUEST_TIMEOUT_MS,
  headers: {
    [HTTP_HEADER.CONTENT_TYPE]: MIME_TYPE.JSON,
  },
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined
    const isUnauthorized = error.response?.status === HTTP_STATUS.UNAUTHORIZED
    const isRefreshRequest = originalRequest?.url === AUTH_ENDPOINTS.REFRESH
    const isRetry = originalRequest?.[RETRY_FLAG] === true

    if (!isUnauthorized || !originalRequest || isRetry || isRefreshRequest) {
      return Promise.reject(error)
    }

    originalRequest[RETRY_FLAG] = true

    const authStore = useAuthStore()
    const refreshed = await authStore.refreshTokens()

    if (!refreshed) {
      authStore.clearLocalSession()
      return Promise.reject(error)
    }

    return api(originalRequest)
  },
)

export default api
