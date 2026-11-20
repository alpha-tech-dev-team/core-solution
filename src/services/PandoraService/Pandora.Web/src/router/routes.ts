import { defineAuthRoute } from '@shared/router/create-auth-router'
import { ROUTES, ROUTE_NAMES } from '@/constants/routes'
import HomeView from '@/views/home/HomeView.vue'
import EmployeesView from '@/views/employees/EmployeesView.vue'
import SecretsView from '@/views/secrets/SecretsView.vue'
import CredentialAccessSettingsView from '@/views/settings/CredentialAccessSettingsView.vue'

export const routes = [
  defineAuthRoute({
    path: ROUTES.HOME,
    name: ROUTE_NAMES.HOME,
    component: HomeView,
  }),
  defineAuthRoute({
    path: ROUTES.EMPLOYEES,
    name: ROUTE_NAMES.EMPLOYEES,
    component: EmployeesView,
  }),
  defineAuthRoute({
    path: ROUTES.SECRETS,
    name: ROUTE_NAMES.SECRETS,
    component: SecretsView,
  }),
  defineAuthRoute({
    path: ROUTES.SETTINGS,
    name: ROUTE_NAMES.SETTINGS,
    component: CredentialAccessSettingsView,
  }),
]
