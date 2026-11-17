export interface AuditEntry {
  id: string
  timestamp: string
  userId: string | null
  telegramUserId: number | null
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

export type SortDirection = 'asc' | 'desc'

export interface AuditQuery {
  search?: string
  action?: string
  requestSource?: string
  sortBy?: string
  sortDirection?: SortDirection
}
