import { createAuditHubConnection } from '@/api/audit-hub'
import type { AuditEntry } from '@/models/audit-entry'

export interface AuditLiveUpdates {
  start(): Promise<void>
  dispose(): void
}

export function useAuditLiveUpdates(onEntry: (entry: AuditEntry) => void): AuditLiveUpdates {
  const connection = createAuditHubConnection(onEntry)

  async function start(): Promise<void> {
    try {
      await connection.start()
    } catch (error) {
      console.error('Не удалось подключиться к каналу аудита', error)
    }
  }

  return {
    start,
    dispose: () => {
      void connection.stop()
    },
  }
}
