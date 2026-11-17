import { API_SORT_DIRECTION } from '@/stores/models/audit-entry'

export const AUDIT_SORT_FIELDS = {
  TIMESTAMP: 'timestamp',
  ACTION: 'action',
  TARGET_ITEM_ID: 'targetItemId',
  REQUEST_SOURCE: 'requestSource'
} as const

export const AUDIT_DEFAULT_SORT_BY: string = AUDIT_SORT_FIELDS.TIMESTAMP
export const AUDIT_DEFAULT_SORT_DIRECTION = API_SORT_DIRECTION.DESCENDING

export const AUDIT_SEARCH_DEBOUNCE_MS = 300

export const AUDIT_SORT_ARIA_VALUES = {
  NONE: 'none',
  ASCENDING: 'ascending',
  DESCENDING: 'descending'
} as const

export type AuditSortAriaValue = (typeof AUDIT_SORT_ARIA_VALUES)[keyof typeof AUDIT_SORT_ARIA_VALUES]

export const AUDIT_SORT_INDICATOR_CLASSES = {
  ASCENDING: 'audit__sort-indicator--ascending',
  DESCENDING: 'audit__sort-indicator--descending'
} as const
