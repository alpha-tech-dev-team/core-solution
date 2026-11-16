import { createSession } from '../auth/session'
import { createSessionGateway } from '../api/session-gateway'
import { createHttpClient } from '../api/http-client'
import { API_BASE_URL, REQUEST_TIMEOUT_MS } from '../constants/api'
import { SHARED_ROUTE_NAMES } from '../constants/routes'

const clientOptions = { baseUrl: API_BASE_URL, timeoutMs: REQUEST_TIMEOUT_MS }

export const session = createSession({
  gateway: createSessionGateway(clientOptions),
  loginRouteName: SHARED_ROUTE_NAMES.LOGIN,
  authenticatedRouteName: SHARED_ROUTE_NAMES.HOME,
})

export const api = createHttpClient(clientOptions)

export const useAuthStore = session.useAuthStore

api.setUnauthorizedHandler(() => useAuthStore().refreshSession())
