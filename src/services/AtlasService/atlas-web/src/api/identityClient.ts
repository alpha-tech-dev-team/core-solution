import config from '../../appsettings.json'
import { HttpMethod, HttpHeader, HttpStatus, MimeType } from '@/constants/http.Constants'
import { messages } from '@/resources'
import type { AuthTokensResponse, LoginRequest, RefreshRequest } from '@/api/contracts'

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
    throw new Error(messages.auth.errorNetwork)
  }

  if (!response.ok) {
    if (response.status === HttpStatus.Unauthorized) {
      throw new Error(unauthorizedMessage)
    }

    const errorData = await response.json().catch(() => null)
    throw new Error(errorData?.message || messages.auth.errorServer)
  }

  return (await response.json()) as T
}

export const identityClient = {
  async login(data: LoginRequest): Promise<AuthTokensResponse> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminLogin}`
    return await postJson<AuthTokensResponse>(url, data, messages.auth.errorInvalidCredentials)
  },

  async refresh(refreshToken: string): Promise<AuthTokensResponse> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminRefresh}`
    const data: RefreshRequest = { refreshToken }
    return await postJson<AuthTokensResponse>(url, data, messages.auth.errorSessionExpired)
  },
}
