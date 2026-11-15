import { watch } from 'vue'
import type { Router } from 'vue-router'
import { ROUTE_META_KEY } from '../constants/routes'

export interface AuthRouteSyncOptions {
  isAuthenticated: () => boolean
  loginRouteName: string
  authenticatedRouteName: string
}

export function syncRoutesWithAuth(router: Router, options: AuthRouteSyncOptions): void {
  watch(options.isAuthenticated, (isAuthenticated) => {
    const route = router.currentRoute.value

    if (!isAuthenticated) {
      if (route.meta[ROUTE_META_KEY.REQUIRES_AUTH]) {
        void router.push({ name: options.loginRouteName })
      }
      return
    }

    if (route.name === options.loginRouteName) {
      void router.push({ name: options.authenticatedRouteName })
    }
  })
}
