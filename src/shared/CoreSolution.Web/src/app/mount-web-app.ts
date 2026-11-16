import { createApp, type Component } from 'vue'
import { createPinia } from 'pinia'
import type { RouteRecordRaw } from 'vue-router'
import { syncRoutesWithAuth } from '../auth/route-sync'
import { createAuthRouter } from '../router/create-auth-router'
import type { AppSession } from './create-app-session'
import type { I18n } from '../i18n/create-i18n'

export interface WebAppOptions {
  root: Component
  routes: RouteRecordRaw[]
  i18n: I18n
  appSession: AppSession
  mountPoint: string
}

export function mountWebApp(options: WebAppOptions): void {
  const { session } = options.appSession
  const router = createAuthRouter({ session, routes: options.routes })
  const app = createApp(options.root)
  const pinia = createPinia()

  app.use(pinia)
  app.use(options.i18n)
  app.use(session)
  app.use(router)

  const authStore = session.useAuthStore(pinia)
  void authStore.restoreSession()

  syncRoutesWithAuth(router, {
    isAuthenticated: () => authStore.isAuthenticated,
    loginRouteName: session.loginRouteName,
    authenticatedRouteName: session.authenticatedRouteName,
  })

  app.mount(options.mountPoint)
}
