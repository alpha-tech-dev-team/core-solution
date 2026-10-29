export const AUTH_ENDPOINTS = {
  LOGIN: '/login',
  REFRESH: '/refresh'
} as const

export const API_BASE_URL = '/api/admin'
export const REQUEST_TIMEOUT_MS = 10_000

export const HEADER_AUTHORIZATION = 'Authorization'
export const HEADER_CONTENT_TYPE = 'Content-Type'
export const CONTENT_TYPE_JSON = 'application/json'
export const AUTH_SCHEME_BEARER = 'Bearer'

export const RETRY_FLAG = '_retry'