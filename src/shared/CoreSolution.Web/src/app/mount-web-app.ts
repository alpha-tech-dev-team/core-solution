import { createApp, type Component } from 'vue'
import { createPinia } from 'pinia'
import type { Router } from 'vue-router'
import { syncRoutesWithAuth } from '../auth/route-sync'
import type { I18n } from '../i18n/create-i18n'
import type { Session } from '../auth/session'

export interface WebAppOptions {
  root: Component
  router: Router
  i18n: I18n
  session: Session
  mountPoint: string
}

export function mountWebApp(options: WebAppOptions): void {
  const app = createApp(options.root)
  const pinia = createPinia()

  app.use(pinia)
  app.use(options.i18n)
  app.use(options.session)
  app.use(options.router)

  const authStore = options.session.useAuthStore(pinia)
  void authStore.restoreSession()

  syncRoutesWithAuth(options.router, {
    isAuthenticated: () => authStore.isAuthenticated,
    loginRouteName: options.session.loginRouteName,
    authenticatedRouteName: options.session.authenticatedRouteName,
  })

  app.mount(options.mountPoint)
}
