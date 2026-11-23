export interface RequireAuthGuardOptions {
  isAuthenticated: () => boolean
  refreshSession: () => Promise<boolean>
  loginRouteName: string
}

export interface RouteRedirect {
  name: string
}

export function createRequireAuthGuard(
  options: RequireAuthGuardOptions,
): () => Promise<true | RouteRedirect> {
  return async () => {
    if (options.isAuthenticated()) return true

    const refreshed = await options.refreshSession()

    return refreshed ? true : { name: options.loginRouteName }
  }
}
