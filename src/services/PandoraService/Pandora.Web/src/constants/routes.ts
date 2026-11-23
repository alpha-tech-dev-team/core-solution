import { SHARED_ROUTE_NAMES, SHARED_ROUTES } from '@shared/constants/routes'

export const ROUTES = {
  ...SHARED_ROUTES,
  EMPLOYEES: '/employees',
  SECRETS: '/secrets',
} as const

export const ROUTE_NAMES = {
  ...SHARED_ROUTE_NAMES,
  EMPLOYEES: 'employees',
  SECRETS: 'secrets',
} as const
