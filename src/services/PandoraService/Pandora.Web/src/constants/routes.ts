export const ROUTES = {
  LOGIN: '/login',
  HOME: '/',
  AUDIT: '/audit'
} as const

export const ROUTE_NAMES = {
  LOGIN: 'login',
  HOME: 'home',
  AUDIT: 'audit'
} as const

export const CATCH_ALL_PATTERN = '/:pathMatch(.*)*'
