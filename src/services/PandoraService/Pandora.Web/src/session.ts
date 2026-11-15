import { createSession } from '@shared/auth/session'
import { createSessionGateway } from '@shared/api/session-gateway'
import { api } from '@/api/client'
import { API_BASE_URL, REQUEST_TIMEOUT_MS } from '@/constants/api'
import { ROUTE_NAMES } from '@/constants/routes'

export const session = createSession({
  gateway: createSessionGateway({ baseUrl: API_BASE_URL, timeoutMs: REQUEST_TIMEOUT_MS }),
  loginRouteName: ROUTE_NAMES.LOGIN,
  authenticatedRouteName: ROUTE_NAMES.HOME,
})

export const useAuthStore = session.useAuthStore

api.setUnauthorizedHandler(() => useAuthStore().refreshSession())
