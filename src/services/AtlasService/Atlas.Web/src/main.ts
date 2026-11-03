import './assets/main.css'

import { createApp, watch } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/authStore'
import { RouteMetaKey, RouteName } from '@/constants/router.Constants'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

const authStore = useAuthStore(pinia)
void authStore.restoreSession()

watch(
  () => authStore.isAuthenticated,
  (authenticated) => {
    const route = router.currentRoute.value

    if (!authenticated) {
      if (route.meta[RouteMetaKey.RequiresAuth]) {
        void router.push({ name: RouteName.Login })
      }
      return
    }

    if (route.name === RouteName.Login) {
      void router.push({ name: RouteName.Admin })
    }
  },
)

app.mount('#app')
