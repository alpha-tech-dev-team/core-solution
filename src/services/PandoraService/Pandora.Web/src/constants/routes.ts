export const ROUTES = {
  LOGIN: '/login',
  HOME: '/',
  SECRETS: '/secrets'
} as const

export const ROUTE_NAMES = {
  LOGIN: 'login',
  HOME: 'home',
  SECRETS: 'secrets'
} as const

export const CATCH_ALL_PATTERN = '/:pathMatch(.*)*'
