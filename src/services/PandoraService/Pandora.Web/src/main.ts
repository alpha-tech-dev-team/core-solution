import '@shared/styles/index.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { syncRoutesWithAuth } from '@shared/auth/route-sync'
import App from './App.vue'
import router from '@/router'
import { i18n } from '@/i18n'
import { session } from '@/session'
import { ROUTE_NAMES } from '@/constants/routes'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(i18n)
app.use(session)
app.use(router)

void session.useAuthStore(pinia).restoreSession()

syncRoutesWithAuth(router, {
  isAuthenticated: () => session.useAuthStore().isAuthenticated,
  loginRouteName: ROUTE_NAMES.LOGIN,
  authenticatedRouteName: ROUTE_NAMES.HOME,
})

app.mount('#app')
