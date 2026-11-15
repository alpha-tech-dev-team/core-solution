export const API_BASE_URL = '/api/admin'
export const REQUEST_TIMEOUT_MS = 10_000

export const AUTH_ENDPOINTS = {
  LOGIN: '/login',
  REFRESH: '/refresh',
  LOGOUT: '/logout',
} as const

export const EMPLOYEE_ENDPOINTS = {
  LIST: '/users',
  BY_ID: (id: string) => `/users/${id}`,
  ROLES: '/roles',
  ROLES_OF: (id: string) => `/users/${id}/roles`,
  STATUS_OF: (id: string) => `/users/${id}/status`,
} as const

export const SECRET_ENDPOINTS = {
  COLLECTION: '/secrets',
  BY_ID: (id: string) => `/secrets/${id}`,
} as const

export const PAGINATION_QUERY = {
  PAGE: 'page',
  PAGE_SIZE: 'pageSize',
} as const

export const RETRY_FLAG = '_retry'

export const REQUEST_ERROR = {
  NETWORK: 'network',
  UNKNOWN: 'unknown',
} as const
