import type { Ref } from 'vue'
import { TOKEN_REFRESH } from '../constants/auth'

export interface SessionTimers {
  start(): void
  stop(): void
  scheduleRefresh(refresh: () => void, expiresAtMs: number): void
  cancelRefresh(): void
  cancelAll(): void
}

export function createSessionTimers(nowMs: Ref<number>): SessionTimers {
  let refreshTimer: number | undefined
  let clockTimer: number | undefined

  function start(): void {
    if (clockTimer !== undefined) return

    clockTimer = window.setInterval(() => {
      nowMs.value = Date.now()
    }, TOKEN_REFRESH.CLOCK_TICK_MS)
  }

  function stop(): void {
    if (clockTimer === undefined) return

    window.clearInterval(clockTimer)
    clockTimer = undefined
  }

  function cancelRefresh(): void {
    if (refreshTimer === undefined) return

    window.clearTimeout(refreshTimer)
    refreshTimer = undefined
  }

  function scheduleRefresh(refresh: () => void, expiresAtMs: number): void {
    cancelRefresh()

    const delay = Math.max(
      expiresAtMs - Date.now() - TOKEN_REFRESH.MARGIN_MS,
      TOKEN_REFRESH.MIN_DELAY_MS,
    )

    refreshTimer = window.setTimeout(refresh, delay)
  }

  return {
    start,
    stop,
    scheduleRefresh,
    cancelRefresh,
    cancelAll: () => {
      cancelRefresh()
      stop()
    },
  }
}
