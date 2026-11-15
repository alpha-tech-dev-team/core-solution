import config from '../../appsettings.json'
import { HTTP_HEADER, HTTP_METHOD, HTTP_STATUS, MIME_TYPE } from '@shared/api/http'
import { ru } from '@/i18n'
import { ApiError } from '@/api/api-error'
import type { ApiErrorResponse } from '@/api/contracts/api-error-response'
import type { LoginRequest } from '@/api/contracts/login-request'
import type { SessionResponse } from '@/api/contracts/session-response'

async function send(
  url: string,
  init: RequestInit,
  unauthorizedMessage: string,
): Promise<Response> {
  let response: Response

  try {
    response = await fetch(url, init)
  } catch {
    throw new ApiError(ru.auth.errorNetwork)
  }

  if (response.ok) return response

  if (response.status === HTTP_STATUS.UNAUTHORIZED) {
    throw new ApiError(unauthorizedMessage, response.status)
  }

  const errorData = (await response.json().catch(() => null)) as ApiErrorResponse | null
  const message = errorData?.message ?? errorData?.detail ?? ru.auth.errorServer

  throw new ApiError(message, response.status)
}

async function postJson<T>(url: string, body: unknown, unauthorizedMessage: string): Promise<T> {
  const response = await send(
    url,
    {
      method: HTTP_METHOD.POST,
      headers: {
        [HTTP_HEADER.CONTENT_TYPE]: MIME_TYPE.JSON,
      },
      body: JSON.stringify(body),
    },
    unauthorizedMessage,
  )

  return (await response.json()) as T
}

export const identityClient = {
  async login(data: LoginRequest): Promise<SessionResponse> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminLogin}`
    return await postJson<SessionResponse>(url, data, ru.auth.errorInvalidCredentials)
  },

  async refresh(): Promise<SessionResponse> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminRefresh}`
    const response = await send(url, { method: HTTP_METHOD.POST }, ru.auth.errorSessionExpired)
    return (await response.json()) as SessionResponse
  },

  async logout(): Promise<void> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminLogout}`
    await send(url, { method: HTTP_METHOD.POST }, ru.auth.errorSessionExpired)
  },
}
