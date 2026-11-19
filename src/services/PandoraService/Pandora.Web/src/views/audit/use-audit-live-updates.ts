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
  let retryTimer: number | undefined
  let retryResolve: (() => void) | undefined

  async function start(): Promise<void> {
    while (!disposed) {
      try {
        await connection.start()

        if (disposed) {
          await connection.stop()
        }

        return
      } catch (error) {
        console.error('Не удалось подключиться к каналу аудита', error)
      }

      await waitBeforeRetry()
    }
  }

  function waitBeforeRetry(): Promise<void> {
    return new Promise((resolve) => {
      retryResolve = resolve
      retryTimer = window.setTimeout(() => {
        retryResolve = undefined
        resolve()
      }, AUDIT_LIVE_RECONNECT_DELAY_MS)
    })
  }

  return {
    start,
    dispose: () => {
      disposed = true
      window.clearTimeout(retryTimer)
      retryResolve?.()
      retryResolve = undefined
      void connection.stop()
    },
  }
}
