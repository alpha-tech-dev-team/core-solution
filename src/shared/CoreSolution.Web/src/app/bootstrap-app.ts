import '../styles/index.css'

import type { RouteRecordRaw } from 'vue-router'
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createI18n, mergeResources, type I18n } from '../i18n/create-i18n'
import { session } from './app-session'
import { createAuthRouter } from '../router/create-auth-router'
import { syncRoutesWithAuth } from '../auth/route-sync'
import { APP_MOUNT_POINT } from '../constants/app'
import CoreApp from './CoreApp.vue'
import sharedEn from '../i18n/locales/en.json'
import sharedRu from '../i18n/locales/ru.json'

export interface AppLocales {
  ru: Record<string, string>
  en: Record<string, string>
}

export interface BootstrapOptions {
  routes: RouteRecordRaw[]
  locales: AppLocales
}

export function bootstrapApp(options: BootstrapOptions): void {
  const i18n: I18n = createI18n({
    resources: mergeResources({ ru: sharedRu, en: sharedEn }, options.locales),
  })
  const router = createAuthRouter({ session, routes: options.routes })
  const app = createApp(CoreApp)
  const pinia = createPinia()

  app.use(pinia)
  app.use(i18n)
  app.use(session)
  app.use(router)

  const authStore = session.useAuthStore(pinia)
  void authStore.restoreSession()

  syncRoutesWithAuth(router, {
    isAuthenticated: () => authStore.isAuthenticated,
    loginRouteName: session.loginRouteName,
    authenticatedRouteName: session.authenticatedRouteName,
  })

  app.mount(APP_MOUNT_POINT)
}
