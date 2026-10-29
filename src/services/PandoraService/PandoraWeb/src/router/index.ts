import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '@/views/LoginView.vue'
import HomeView from '@/views/HomeView.vue'
import { useAuthStore } from '@/stores/auth'
import { CATCH_ALL_PATTERN, ROUTE_NAMES, ROUTES } from './constants'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: ROUTES.LOGIN,
      name: ROUTE_NAMES.LOGIN,
      component: LoginView
    },
    {
      path: ROUTES.HOME,
      name: ROUTE_NAMES.HOME,
      component: HomeView,
      beforeEnter: () => {
        const authStore = useAuthStore()
        return authStore.isAuthenticated ? true : ROUTES.LOGIN
      }
    },
    {
      path: CATCH_ALL_PATTERN,
      redirect: ROUTES.HOME
    }
  ]
})

export default router