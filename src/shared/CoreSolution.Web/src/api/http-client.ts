import { ApiError } from './api-error'
import { HTTP_HEADER, HTTP_METHOD, HTTP_STATUS, MIME_TYPE } from './http'

export type HttpMethod = (typeof HTTP_METHOD)[keyof typeof HTTP_METHOD]

export type QueryParams = Record<string, string | number>

export interface HttpClientOptions {
  baseUrl: string
  timeoutMs: number
}

export interface HttpClient {
  get<T>(path: string, query?: QueryParams): Promise<T>
  post<T>(path: string, body?: unknown): Promise<T>
  put<T>(path: string, body?: unknown): Promise<T>
  patch<T>(path: string, body?: unknown): Promise<T>
  delete<T>(path: string): Promise<T>
  setUnauthorizedHandler(handler: () => Promise<boolean>): void
}

interface RequestOptions {
  path: string
  method: HttpMethod
  body?: unknown
  query?: QueryParams
}

interface ErrorBody {
  code?: string
}

function buildUrl(baseUrl: string, path: string, query?: QueryParams): string {
  if (query === undefined) return `${baseUrl}${path}`

  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(query)) params.set(key, String(value))

  return `${baseUrl}${path}?${params.toString()}`
}

async function readErrorCode(response: Response): Promise<string | null> {
  const body = (await response.json().catch(() => null)) as ErrorBody | null

  return typeof body?.code === 'string' ? body.code : null
}

async function readBody<T>(response: Response): Promise<T> {
  if (response.status === HTTP_STATUS.NO_CONTENT) return undefined as T

  const text = await response.text()

  return (text === '' ? undefined : JSON.parse(text)) as T
}

function buildRequestInit(request: RequestOptions, timeoutMs: number): RequestInit {
  return {
    method: request.method,
    headers:
      request.body === undefined ? undefined : { [HTTP_HEADER.CONTENT_TYPE]: MIME_TYPE.JSON },
    body: request.body === undefined ? undefined : JSON.stringify(request.body),
    signal: AbortSignal.timeout(timeoutMs),
  }
}

export function createHttpClient(options: HttpClientOptions): HttpClient {
  let unauthorizedHandler: (() => Promise<boolean>) | null = null

  async function send(request: RequestOptions, isRetry: boolean): Promise<Response> {
    let response: Response

    try {
      response = await fetch(
        buildUrl(options.baseUrl, request.path, request.query),
        buildRequestInit(request, options.timeoutMs),
      )
    } catch {
      throw new ApiError(null)
    }

    if (response.ok) return response

    if (
      response.status === HTTP_STATUS.UNAUTHORIZED &&
      !isRetry &&
      unauthorizedHandler !== null &&
      (await unauthorizedHandler())
    ) {
      return await send(request, true)
    }

    throw new ApiError(response.status, await readErrorCode(response))
  }

  async function request<T>(requestOptions: RequestOptions): Promise<T> {
    const response = await send(requestOptions, false)

    return await readBody<T>(response)
  }

  return {
    get: async <T>(path: string, query?: QueryParams) =>
      await request<T>({ path, method: HTTP_METHOD.GET, query }),
    post: async <T>(path: string, body?: unknown) =>
      await request<T>({ path, method: HTTP_METHOD.POST, body }),
    put: async <T>(path: string, body?: unknown) =>
      await request<T>({ path, method: HTTP_METHOD.PUT, body }),
    patch: async <T>(path: string, body?: unknown) =>
      await request<T>({ path, method: HTTP_METHOD.PATCH, body }),
    delete: async <T>(path: string) => await request<T>({ path, method: HTTP_METHOD.DELETE }),
    setUnauthorizedHandler: (handler) => {
      unauthorizedHandler = handler
    },
  }
}
