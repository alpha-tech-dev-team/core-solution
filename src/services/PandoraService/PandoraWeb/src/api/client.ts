import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth'
import {
  API_BASE_URL,
  AUTH_SCHEME_BEARER,
  CONTENT_TYPE_JSON,
  HEADER_AUTHORIZATION,
  HEADER_CONTENT_TYPE,
  REQUEST_TIMEOUT_MS,
  RETRY_FLAG
} from '@/api/constants'
import { HTTP_STATUS } from '@/api/http-status'

type RetriableRequestConfig = AxiosRequestConfig & { [RETRY_FLAG]?: boolean }

const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: REQUEST_TIMEOUT_MS,
  headers: {
    [HEADER_CONTENT_TYPE]: CONTENT_TYPE_JSON
  }
})

api.interceptors.request.use(
  (config) => {
    const { accessToken } = useAuthStore()
    if (accessToken) {
      config.headers.set(HEADER_AUTHORIZATION, `${AUTH_SCHEME_BEARER} ${accessToken}`)
    }
    return config
  },
  (error) => Promise.reject(error)
)

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined
    const isUnauthorized = error.response?.status === HTTP_STATUS.UNAUTHORIZED

    if (!isUnauthorized || !originalRequest || originalRequest[RETRY_FLAG]) {
      return Promise.reject(error)
    }

    originalRequest[RETRY_FLAG] = true
    const authStore = useAuthStore()
    const refreshed = await authStore.refreshTokens()

    if (!refreshed) {
      authStore.logout()
      return Promise.reject(error)
    }

    return api(originalRequest)
  }
)

export default api