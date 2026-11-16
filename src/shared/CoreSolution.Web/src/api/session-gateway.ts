import { AUTH_ENDPOINTS } from '../constants/api'
import { createHttpClient, type HttpClient } from './http-client'
import type { SessionGateway } from '../auth/session'
import type { ApiSession } from '../models/api-session'
import type { Credentials } from '../models/credentials'

export interface SessionGatewayOptions {
  baseUrl: string
  timeoutMs: number
}

export function createSessionGateway(options: SessionGatewayOptions): SessionGateway {
  const client = createHttpClient(options)

  return {
    login: async (credentials: Credentials) =>
      await client.post<ApiSession>(AUTH_ENDPOINTS.LOGIN, credentials),
    refresh: async () => await client.post<ApiSession>(AUTH_ENDPOINTS.REFRESH),
    logout: async () => {
      await client.post(AUTH_ENDPOINTS.LOGOUT)
    },
  }
}

export function createAuthClient(options: SessionGatewayOptions): HttpClient {
  return createHttpClient(options)
}
