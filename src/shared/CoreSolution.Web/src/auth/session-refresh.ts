import { HTTP_STATUS } from '../api/http'
import { ApiError } from '../api/api-error'
import type { ApiSession } from '../models/api-session'

export interface RefreshCoordinatorOptions {
  refresh: () => Promise<ApiSession>
  currentVersion: () => number
  applyRefreshed: (apiSession: ApiSession) => void
  clear: () => void
}

export interface RefreshCoordinator {
  refresh(): Promise<boolean>
}

export function createRefreshCoordinator(options: RefreshCoordinatorOptions): RefreshCoordinator {
  let pending: Promise<boolean> | null = null

  async function perform(): Promise<boolean> {
    const version = options.currentVersion()

    try {
      const refreshed = await options.refresh()
      if (options.currentVersion() !== version) return false

      options.applyRefreshed(refreshed)

      return true
    } catch (error) {
      if (error instanceof ApiError && error.status === HTTP_STATUS.UNAUTHORIZED) options.clear()

      return false
    }
  }

  return {
    refresh: () => {
      if (pending === null) {
        pending = perform().finally(() => {
          pending = null
        })
      }

      return pending
    },
  }
}
