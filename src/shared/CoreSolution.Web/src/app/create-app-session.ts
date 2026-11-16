import { createHttpClient, type HttpClient } from '../api/http-client'
import { createSessionGateway } from '../api/session-gateway'
import { createSession, type Session } from '../auth/session'
import { API_BASE_URL, REQUEST_TIMEOUT_MS } from '../constants/api'
import { SHARED_ROUTE_NAMES } from '../constants/routes'

export interface AppSession {
  session: Session
  api: HttpClient
}

export function createAppSession(): AppSession {
  const clientOptions = { baseUrl: API_BASE_URL, timeoutMs: REQUEST_TIMEOUT_MS }

  const session = createSession({
    gateway: createSessionGateway(clientOptions),
    loginRouteName: SHARED_ROUTE_NAMES.LOGIN,
    authenticatedRouteName: SHARED_ROUTE_NAMES.HOME,
  })

  const api = createHttpClient(clientOptions)
  api.setUnauthorizedHandler(() => session.useAuthStore().refreshSession())

  return { session, api }
}
