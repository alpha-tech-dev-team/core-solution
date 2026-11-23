export const ROUTE_META_KEY = {
  REQUIRES_AUTH: 'requiresAuth',
} as const

export const SHARED_ROUTES = {
  LOGIN: '/login',
  HOME: '/',
} as const

export const SHARED_ROUTE_NAMES = {
  LOGIN: 'login',
  HOME: 'home',
} as const

export const CATCH_ALL_PATTERN = '/:pathMatch(.*)*'
