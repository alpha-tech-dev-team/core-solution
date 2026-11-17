import type { SortDirection } from '@/stores/models/audit-entry'

export const AUDIT_SORT_FIELDS = {
  TIMESTAMP: 'timestamp',
  ACTION: 'action',
  TARGET_ITEM_ID: 'targetItemId',
  REQUEST_SOURCE: 'requestSource'
} as const

export const AUDIT_DEFAULT_SORT_BY: string = AUDIT_SORT_FIELDS.TIMESTAMP
export const AUDIT_DEFAULT_SORT_DIRECTION: SortDirection = 'desc'

export const AUDIT_SEARCH_DEBOUNCE_MS = 300
