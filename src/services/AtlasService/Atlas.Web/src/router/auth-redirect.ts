import { watch } from 'vue'
import type { Router } from 'vue-router'
import { ROUTE_META_KEY, ROUTE_NAME } from '@/constants/router'

export function syncRoutesWithAuth(router: Router, isAuthenticated: () => boolean): void {
  watch(isAuthenticated, (authenticated) => {
    const route = router.currentRoute.value

    if (!authenticated) {
      if (route.meta[ROUTE_META_KEY.REQUIRES_AUTH]) {
        void router.push({ name: ROUTE_NAME.LOGIN })
      }
      return
    }

    if (route.name === ROUTE_NAME.LOGIN) {
      void router.push({ name: ROUTE_NAME.ADMIN })
    }
  })
}
