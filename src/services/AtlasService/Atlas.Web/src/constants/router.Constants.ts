export const RouteName = {
  Login: 'login',
  Admin: 'admin',
} as const

export const RoutePath = {
  Login: '/',
  Admin: '/admin',
  CatchAll: '/:pathMatch(.*)*',
} as const

export const RouteMetaKey = {
  RequiresAuth: 'requiresAuth',
} as const
