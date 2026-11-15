import { createRouter, createWebHistory } from 'vue-router'
import { createRequireAuthGuard } from '@shared/auth/auth-guard'
import LoginView from '@/views/login/LoginView.vue'
import HomeView from '@/views/home/HomeView.vue'
import EmployeesView from '@/views/employees/EmployeesView.vue'
import SecretsView from '@/views/secrets/SecretsView.vue'
import { useAuthStore } from '@/stores/auth'
import { CATCH_ALL_PATTERN, ROUTES, ROUTE_NAMES } from '@/constants/routes'

const requireAuth = createRequireAuthGuard({
  isAuthenticated: () => useAuthStore().isAuthenticated,
  refreshSession: () => useAuthStore().refreshTokens(),
  loginRouteName: ROUTE_NAMES.LOGIN,
})

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: ROUTES.LOGIN,
      name: ROUTE_NAMES.LOGIN,
      component: LoginView,
    },
    {
      path: ROUTES.HOME,
      name: ROUTE_NAMES.HOME,
      component: HomeView,
      beforeEnter: requireAuth,
    },
    {
      path: ROUTES.EMPLOYEES,
      name: ROUTE_NAMES.EMPLOYEES,
      component: EmployeesView,
      beforeEnter: requireAuth,
    },
    {
      path: ROUTES.SECRETS,
      name: ROUTE_NAMES.SECRETS,
      component: SecretsView,
      beforeEnter: requireAuth,
    },
    {
      path: CATCH_ALL_PATTERN,
      redirect: { name: ROUTE_NAMES.HOME },
    },
  ],
})

export default router
