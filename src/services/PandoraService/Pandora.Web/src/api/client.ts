import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth'
import {
  API_BASE_URL,
  AUTH_ENDPOINTS,
  CONTENT_TYPE_JSON,
  HEADER_CONTENT_TYPE,
  REQUEST_TIMEOUT_MS,
  RETRY_FLAG
} from '@/constants/api'
import { HTTP_STATUS } from '@/constants/http-status'

type RetriableRequestConfig = AxiosRequestConfig & { [RETRY_FLAG]?: boolean }

const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: REQUEST_TIMEOUT_MS,
  headers: {
    [HEADER_CONTENT_TYPE]: CONTENT_TYPE_JSON
  }
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined
    const isUnauthorized = error.response?.status === HTTP_STATUS.UNAUTHORIZED
    const isRefreshRequest = originalRequest?.url === AUTH_ENDPOINTS.REFRESH

    if (!isUnauthorized || !originalRequest || originalRequest[RETRY_FLAG] || isRefreshRequest) {
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
  }
)

export default api
