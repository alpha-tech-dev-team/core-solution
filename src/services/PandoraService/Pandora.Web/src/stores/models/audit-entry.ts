export interface AuditEntry {
  id: string
  timestamp: string
  telegramUserId: number
  action: string
  targetItemId: string | null
  requestSource: string
}

export interface AuditEntryPage {
  items: AuditEntry[]
  page: number
  pageSize: number
  total: number
}
