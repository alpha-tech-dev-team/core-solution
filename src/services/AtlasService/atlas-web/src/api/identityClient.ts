import config from '../../appsettings.json'
import { HttpMethod, HttpHeader, MimeType } from '@/constants/http.Constants'
import { messages } from '@/resources'
import type { AuthTokensResponse, LoginRequest } from '@/api/contracts'

async function postJson<T>(url: string, body: unknown): Promise<T> {
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
    const errorData = await response.json().catch(() => null)
    throw new Error(errorData?.message || messages.auth.errorServer)
  }

  return (await response.json()) as T
}

export const identityClient = {
  async login(data: LoginRequest): Promise<AuthTokensResponse> {
    const url = `${config.IdentityApiBaseUrl}${config.endpoints.adminLogin}`
    return await postJson<AuthTokensResponse>(url, data)
  },
}
