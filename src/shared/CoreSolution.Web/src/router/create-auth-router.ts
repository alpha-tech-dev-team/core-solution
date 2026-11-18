import { createRouter, createWebHistory, type RouteRecordRaw, type Router } from 'vue-router'
import type { Session } from '../auth/session'
import {
  CATCH_ALL_PATTERN,
  ROUTE_META_KEY,
  SHARED_ROUTES,
  SHARED_ROUTE_NAMES,
} from '../constants/routes'
import LoginView from '../views/login/LoginView.vue'

export interface AuthRouterOptions {
  session: Session
  routes: RouteRecordRaw[]
  basePath?: string
}

export function defineAuthRoute(route: RouteRecordRaw): RouteRecordRaw {
  return { ...route, meta: { ...route.meta, [ROUTE_META_KEY.REQUIRES_AUTH]: true } }
}

const loginRoute: RouteRecordRaw = {
  path: SHARED_ROUTES.LOGIN,
  name: SHARED_ROUTE_NAMES.LOGIN,
  component: LoginView,
}

const catchAllRoute: RouteRecordRaw = {
  path: CATCH_ALL_PATTERN,
  redirect: { name: SHARED_ROUTE_NAMES.LOGIN },
}

export function createAuthRouter(options: AuthRouterOptions): Router {
  const router = createRouter({
    history: createWebHistory(options.basePath ?? import.meta.env.BASE_URL),
    routes: [loginRoute, ...options.routes, catchAllRoute],
  })

  router.beforeEach(async (to) => {
    if (to.meta[ROUTE_META_KEY.REQUIRES_AUTH]) return await options.session.requireAuth()

    if (to.name === SHARED_ROUTE_NAMES.LOGIN && options.session.useAuthStore().isAuthenticated) {
      return { name: SHARED_ROUTE_NAMES.HOME }
    }

    return undefined
  })

  return router
}
