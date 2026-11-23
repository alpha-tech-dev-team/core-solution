import { SHARED_ROUTE_NAMES, SHARED_ROUTES } from '@shared/constants/routes'

export const ROUTES = {
  ...SHARED_ROUTES,
  INCIDENTS: '/incidents',
  ROOMS: '/rooms',
} as const

export const ROUTE_NAMES = {
  ...SHARED_ROUTE_NAMES,
  INCIDENTS: 'incidents',
  ROOMS: 'rooms',
} as const
