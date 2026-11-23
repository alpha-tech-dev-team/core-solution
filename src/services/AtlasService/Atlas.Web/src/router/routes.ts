import { defineAuthRoute } from '@shared/router/create-auth-router'
import { ROUTE_NAMES, ROUTES } from '@/constants/routes'
import HomeView from '@/views/home/HomeView.vue'
import IncidentsView from '@/views/incidents/IncidentsView.vue'
import RoomsView from '@/views/rooms/RoomsView.vue'

export const routes = [
  defineAuthRoute({
    path: ROUTES.HOME,
    name: ROUTE_NAMES.HOME,
    component: HomeView,
  }),
  defineAuthRoute({
    path: ROUTES.INCIDENTS,
    name: ROUTE_NAMES.INCIDENTS,
    component: IncidentsView,
  }),
  defineAuthRoute({
    path: ROUTES.ROOMS,
    name: ROUTE_NAMES.ROOMS,
    component: RoomsView,
  }),
]
