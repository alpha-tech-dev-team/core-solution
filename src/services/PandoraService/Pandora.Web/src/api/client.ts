import { createHttpClient } from '@shared/api/http-client'
import { API_BASE_URL, REQUEST_TIMEOUT_MS } from '@/constants/api'

export const api = createHttpClient({
  baseUrl: API_BASE_URL,
  timeoutMs: REQUEST_TIMEOUT_MS,
})
