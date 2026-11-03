export const ROUTES = {
  LOGIN: '/login',
  HOME: '/'
} as const

export const ROUTE_NAMES = {
  LOGIN: 'login',
  HOME: 'home'
} as const

export const CATCH_ALL_PATTERN = '/:pathMatch(.*)*'