export const ROUTE_NAME = {
  LOGIN: 'login',
  ADMIN: 'admin',
} as const

export const ROUTE_PATH = {
  LOGIN: '/',
  ADMIN: '/admin',
  CATCH_ALL: '/:pathMatch(.*)*',
} as const

export const ROUTE_META_KEY = {
  REQUIRES_AUTH: 'requiresAuth',
} as const
