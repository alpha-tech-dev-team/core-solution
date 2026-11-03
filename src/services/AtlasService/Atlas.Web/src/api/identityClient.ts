import config from '../../appsettings.json'
import { HttpMethod, HttpHeader, HttpStatus, MimeType } from '@/constants/http.Constants'
import { Atlas } from '@/resources'
import { ApiError } from '@/api/ApiError'
import type { ApiErrorResponse } from '@/api/contracts/ApiErrorResponse'
import type { AuthTokensResponse } from '@/api/contracts/AuthTokensResponse'
import type { LoginRequest } from '@/api/contracts/LoginRequest'
import type { RefreshRequest } from '@/api/contracts/RefreshRequest'

async function postJson<T>(url: string, body: unknown, unauthorizedMessage: string): Promise<T> {
  let response: Response
  try {
    response = await fetch(url, {
      method: HttpMethod.POST,
      headers: {
        [HttpHeader.ContentType]: MimeType.Json,
      },
      body: JSON.stringify(body),
    })
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

  return (await response.json()) as T
}

export const identityClient = {
  async login(data: LoginRequest): Promise<AuthTokensResponse> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminLogin}`
    return await postJson<AuthTokensResponse>(url, data, Atlas.ru.auth.errorInvalidCredentials)
  },

  async refresh(refreshToken: string): Promise<AuthTokensResponse> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminRefresh}`
    const data: RefreshRequest = { refreshToken }
    return await postJson<AuthTokensResponse>(url, data, Atlas.ru.auth.errorSessionExpired)
  },
}
