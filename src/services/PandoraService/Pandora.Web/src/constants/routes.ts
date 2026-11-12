export const ROUTES = {
  LOGIN: '/login',
  HOME: '/',
  AUDIT: '/audit',
  EMPLOYEES: '/employees',
  SECRETS: '/secrets'
} as const

export const ROUTE_NAMES = {
  LOGIN: 'login',
  HOME: 'home',
  AUDIT: 'audit',
  EMPLOYEES: 'employees',
  SECRETS: 'secrets'
} as const

export const CATCH_ALL_PATTERN = '/:pathMatch(.*)*'
