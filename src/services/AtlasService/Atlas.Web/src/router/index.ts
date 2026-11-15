import { createRouter, createWebHistory } from 'vue-router'
import { createRequireAuthGuard } from '@shared/auth/auth-guard'
import LoginView from '@/views/login/LoginView.vue'
import AdminView from '@/views/admin/AdminView.vue'
import { ROUTE_META_KEY, ROUTE_NAME, ROUTE_PATH } from '@/constants/router'
import { useAuthStore } from '@/stores/auth'

const requireAuth = createRequireAuthGuard({
  isAuthenticated: () => useAuthStore().isAuthenticated,
  refreshSession: () => useAuthStore().refreshSession(),
  loginRouteName: ROUTE_NAME.LOGIN,
})

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: ROUTE_PATH.LOGIN,
      name: ROUTE_NAME.LOGIN,
      component: LoginView,
    },
    {
      path: ROUTE_PATH.ADMIN,
      name: ROUTE_NAME.ADMIN,
      component: AdminView,
      meta: { [ROUTE_META_KEY.REQUIRES_AUTH]: true },
    },
    {
      path: ROUTE_PATH.CATCH_ALL,
      redirect: { name: ROUTE_NAME.LOGIN },
    },
  ],
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()

  if (to.meta[ROUTE_META_KEY.REQUIRES_AUTH]) return await requireAuth()

  if (to.name === ROUTE_NAME.LOGIN && authStore.isAuthenticated) {
    return { name: ROUTE_NAME.ADMIN }
  }
})

export default router
