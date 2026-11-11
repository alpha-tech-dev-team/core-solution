export const ROUTES = {
  LOGIN: '/login',
  HOME: '/',
  EMPLOYEES: '/employees'
} as const

export const ROUTE_NAMES = {
  LOGIN: 'login',
  HOME: 'home',
  EMPLOYEES: 'employees'
} as const

export const CATCH_ALL_PATTERN = '/:pathMatch(.*)*'
