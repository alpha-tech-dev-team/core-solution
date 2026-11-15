import { createRouter, createWebHistory } from 'vue-router'
import { ROUTE_META_KEY } from '@shared/constants/routes'
import LoginView from '@shared/views/login/LoginView.vue'
import HomeView from '@/views/home/HomeView.vue'
import EmployeesView from '@/views/employees/EmployeesView.vue'
import SecretsView from '@/views/secrets/SecretsView.vue'
import { CATCH_ALL_PATTERN, ROUTES, ROUTE_NAMES } from '@/constants/routes'
import { session, useAuthStore } from '@/session'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
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
      meta: { [ROUTE_META_KEY.REQUIRES_AUTH]: true },
    },
    {
      path: ROUTES.EMPLOYEES,
      name: ROUTE_NAMES.EMPLOYEES,
      component: EmployeesView,
      meta: { [ROUTE_META_KEY.REQUIRES_AUTH]: true },
    },
    {
      path: ROUTES.SECRETS,
      name: ROUTE_NAMES.SECRETS,
      component: SecretsView,
      meta: { [ROUTE_META_KEY.REQUIRES_AUTH]: true },
    },
    {
      path: CATCH_ALL_PATTERN,
      redirect: { name: ROUTE_NAMES.LOGIN },
    },
  ],
})

router.beforeEach(async (to) => {
  if (to.meta[ROUTE_META_KEY.REQUIRES_AUTH]) return await session.requireAuth()

  if (to.name === ROUTE_NAMES.LOGIN && useAuthStore().isAuthenticated) {
    return { name: ROUTE_NAMES.HOME }
  }
})

export default router
