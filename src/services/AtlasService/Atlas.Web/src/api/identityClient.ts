import config from '../../appsettings.json'
import { HttpMethod, HttpHeader, HttpStatus, MimeType } from '@/constants/http.Constants'
import { Atlas } from '@/resources'
import { ApiError } from '@/api/ApiError'
import type { ApiErrorResponse } from '@/api/contracts/ApiErrorResponse'
import type { SessionResponse } from '@/api/contracts/SessionResponse'
import type { LoginRequest } from '@/api/contracts/LoginRequest'

async function send(
  url: string,
  init: RequestInit,
  unauthorizedMessage: string,
): Promise<Response> {
  let response: Response
  try {
    response = await fetch(url, init)
  } catch {
    throw new ApiError(Atlas.ru.auth.errorNetwork)
  }

  if (!response.ok) {
    if (response.status === HttpStatus.Unauthorized) {
      throw new ApiError(unauthorizedMessage, response.status)
    }

    const errorData = (await response.json().catch(() => null)) as ApiErrorResponse | null
    const message = errorData?.message ?? errorData?.detail ?? Atlas.ru.auth.errorServer
    throw new ApiError(message, response.status)
  }

  return response
}

async function postJson<T>(
  url: string,
  body: unknown,
  unauthorizedMessage: string,
): Promise<T> {
  const response = await send(
    url,
    {
      method: HttpMethod.POST,
      headers: {
        [HttpHeader.ContentType]: MimeType.Json,
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
    return await postJson<SessionResponse>(url, data, Atlas.ru.auth.errorInvalidCredentials)
  },

  async refresh(): Promise<SessionResponse> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminRefresh}`
    const response = await send(
      url,
      { method: HttpMethod.POST },
      Atlas.ru.auth.errorSessionExpired,
    )
    return (await response.json()) as SessionResponse
  },

  async logout(): Promise<void> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminLogout}`
    await send(url, { method: HttpMethod.POST }, Atlas.ru.auth.errorSessionExpired)
  },
}
