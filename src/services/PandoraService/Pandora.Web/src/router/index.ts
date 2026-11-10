import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '@/views/LoginView.vue'
import HomeView from '@/views/HomeView.vue'
import AuditView from '@/views/AuditView.vue'
import { useAuthStore } from '@/stores/auth'
import { CATCH_ALL_PATTERN, ROUTE_NAMES, ROUTES } from '@/constants/routes'

async function requireSession(): Promise<boolean | string> {
  const authStore = useAuthStore()
  if (authStore.isAuthenticated) return true
  const refreshed = await authStore.refreshTokens()
  return refreshed ? true : ROUTES.LOGIN
}

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
      beforeEnter: requireSession
    },
    {
      path: ROUTES.AUDIT,
      name: ROUTE_NAMES.AUDIT,
      component: AuditView,
      beforeEnter: requireSession
    },
    {
      path: CATCH_ALL_PATTERN,
      redirect: ROUTES.HOME
    }
  ]
})

export default router
