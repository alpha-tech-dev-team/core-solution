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

export const EMPLOYEE_SECRET_ENDPOINTS = {
  LIST: (telegramUserId: number) => `/employees/${telegramUserId}/secrets`,
  BY_ID: (telegramUserId: number, itemId: string) =>
    `/employees/${telegramUserId}/secrets/${itemId}`,
} as const

export const PAGINATION_QUERY = {
  PAGE: 'page',
  PAGE_SIZE: 'pageSize',
  INCLUDE_ARCHIVED: 'includeArchived',
} as const
