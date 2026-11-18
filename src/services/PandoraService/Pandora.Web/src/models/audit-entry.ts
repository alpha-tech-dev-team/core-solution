export const API_SORT_DIRECTION = {
  ASCENDING: 'Ascending',
  DESCENDING: 'Descending',
} as const

export type ApiSortDirection = (typeof API_SORT_DIRECTION)[keyof typeof API_SORT_DIRECTION]

export interface AuditEntry {
  id: string
  timestamp: string
  userId: string | null
  telegramUserId: number | null
  action: string
  targetItemId: string | null
  requestSource: string
}

export interface AuditQuery {
  search?: string
  action?: string
  requestSource?: string
  sortBy?: string
  sortDirection?: ApiSortDirection
}
