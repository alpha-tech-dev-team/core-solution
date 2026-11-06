import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '@/views/LoginView.vue'
import HomeView from '@/views/HomeView.vue'
import EmployeesView from '@/views/employees/EmployeesView.vue'
import { useAuthStore } from '@/stores/auth'
import { CATCH_ALL_PATTERN, ROUTE_NAMES, ROUTES } from '@/constants/routes'

async function requireAuth() {
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
      beforeEnter: requireAuth
    },
    {
      path: ROUTES.EMPLOYEES,
      name: ROUTE_NAMES.EMPLOYEES,
      component: EmployeesView,
      beforeEnter: requireAuth
    },
    {
      path: CATCH_ALL_PATTERN,
      redirect: ROUTES.HOME
    }
  ]
})

export default router
