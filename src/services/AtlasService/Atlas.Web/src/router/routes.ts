import { defineAuthRoute } from '@shared/router/create-auth-router'
import { SHARED_ROUTE_NAMES, SHARED_ROUTES } from '@shared/constants/routes'
import AdminView from '@/views/admin/AdminView.vue'

export const routes = [
  defineAuthRoute({
    path: SHARED_ROUTES.HOME,
    name: SHARED_ROUTE_NAMES.HOME,
    component: AdminView,
  }),
]
