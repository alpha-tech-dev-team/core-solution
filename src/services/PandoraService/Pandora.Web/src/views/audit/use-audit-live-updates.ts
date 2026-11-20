import { createAuditHubConnection } from '@/api/audit-hub'
import { AUDIT_LIVE_RECONNECT_DELAY_MS } from '@/constants/audit'
import type { AuditEntry } from '@/models/audit-entry'

export interface AuditLiveUpdates {
  start(): Promise<void>
  dispose(): void
}

export function useAuditLiveUpdates(onEntry: (entry: AuditEntry) => void): AuditLiveUpdates {
  const connection = createAuditHubConnection(onEntry)
  let disposed = false
  let loopActive = false
  let reconnectPending = false
  let retryTimer: number | undefined
  let retryResolve: (() => void) | undefined

  function settleRetry(): void {
    if (retryTimer !== undefined) {
      window.clearTimeout(retryTimer)
      retryTimer = undefined
    }
    reconnectPending = false
    retryResolve?.()
    retryResolve = undefined
  }

  function waitBeforeRetry(): Promise<boolean> {
    reconnectPending = true

    return new Promise((resolve) => {
      retryResolve = () => resolve(!disposed)
      retryTimer = window.setTimeout(settleRetry, AUDIT_LIVE_RECONNECT_DELAY_MS)
    })
  }

  async function start(): Promise<void> {
    if (loopActive || disposed) return

    loopActive = true

    try {
      while (!disposed) {
        try {
          await connection.start()

          loopActive = false

          if (disposed) {
            await connection.stop()
          }

          return
        } catch (error) {
          console.error('Не удалось подключиться к каналу аудита', error)
        }

        if (disposed) return
        if (!(await waitBeforeRetry())) return
      }
    } finally {
      loopActive = false
    }
  }

  connection.onclose(() => {
    if (disposed || loopActive || reconnectPending) return

    void waitBeforeRetry().then((stillActive) => {
      if (stillActive) void start()
    })
  })

  return {
    start,
    dispose: () => {
      disposed = true
      settleRetry()
      void connection.stop()
    },
  }
}
