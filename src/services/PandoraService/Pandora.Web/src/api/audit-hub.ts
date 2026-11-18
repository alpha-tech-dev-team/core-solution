import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr'
import type { AuditEntry } from '@/models/audit-entry'

const AUDIT_HUB_URL = '/hubs/audit'
const RECEIVE_AUDIT_ENTRY = 'ReceiveAuditEntry'

export function createAuditHubConnection(onEntry: (entry: AuditEntry) => void): HubConnection {
  const connection = new HubConnectionBuilder()
    .withUrl(AUDIT_HUB_URL)
    .withAutomaticReconnect()
    .build()

  connection.on(RECEIVE_AUDIT_ENTRY, onEntry)

  return connection
}
