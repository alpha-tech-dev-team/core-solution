import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '@/views/LoginForm.vue'
import { RouteName, RoutePath } from '@/constants/router.Constants'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: RoutePath.Login,
      name: RouteName.Login,
      component: LoginForm,
    },
  ],
})

export default router
