import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '@/views/LoginForm.vue'
import AdminForm from '@/views/AdminForm.vue'
import { RouteName, RoutePath } from '@/constants/router.Constants'
import { useAuthStore } from '@/stores/authStore'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: RoutePath.Login,
      name: RouteName.Login,
      component: LoginForm,
    },
    {
      path: RoutePath.Admin,
      name: RouteName.Admin,
      component: AdminForm,
      meta: { requiresAuth: true },
    },
  ],
})

router.beforeEach((to) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: RouteName.Login }
  }

  if (to.name === RouteName.Login && authStore.isAuthenticated) {
    return { name: RouteName.Admin }
  }
})

export default router
