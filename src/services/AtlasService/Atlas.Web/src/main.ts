import '@/styles/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { syncRoutesWithAuth } from '@/router/auth-redirect'
import { useAuthStore } from '@/stores/auth'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

const authStore = useAuthStore(pinia)
void authStore.restoreSession()

syncRoutesWithAuth(router, () => authStore.isAuthenticated)

app.mount('#app')
