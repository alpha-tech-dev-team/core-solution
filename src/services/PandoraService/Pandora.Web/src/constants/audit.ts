import { API_SORT_DIRECTION } from '@/models/audit-entry'

export const AUDIT_SORT_FIELDS = {
  TIMESTAMP: 'Timestamp',
  ACTION: 'Action',
  TARGET_ITEM_ID: 'TargetItemId',
  REQUEST_SOURCE: 'RequestSource',
} as const

export const AUDIT_DEFAULT_SORT_BY: string = AUDIT_SORT_FIELDS.TIMESTAMP
export const AUDIT_DEFAULT_SORT_DIRECTION = API_SORT_DIRECTION.DESCENDING

export const AUDIT_SEARCH_DEBOUNCE_MS = 300
export const AUDIT_LIVE_REFRESH_DEBOUNCE_MS = 300
export const AUDIT_LIVE_RECONNECT_DELAY_MS = 5_000

export const AUDIT_ACTION_KEY_PREFIX = 'audit_action'
export const AUDIT_SOURCE_KEY_PREFIX = 'audit_source'

export const AUDIT_ACTION_KEYS = {
  submit_phone: `${AUDIT_ACTION_KEY_PREFIX}_submit_phone`,
  show_services: `${AUDIT_ACTION_KEY_PREFIX}_show_services`,
  get_credentials: `${AUDIT_ACTION_KEY_PREFIX}_get_credentials`,
  secret_created: `${AUDIT_ACTION_KEY_PREFIX}_secret_created`,
  secret_updated: `${AUDIT_ACTION_KEY_PREFIX}_secret_updated`,
  secret_deleted: `${AUDIT_ACTION_KEY_PREFIX}_secret_deleted`,
} as const

export const AUDIT_SOURCE_KEYS = {
  telegram: `${AUDIT_SOURCE_KEY_PREFIX}_telegram`,
  web: `${AUDIT_SOURCE_KEY_PREFIX}_web`,
} as const

export const AUDIT_SORT_ARIA_VALUES = {
  NONE: 'none',
  ASCENDING: 'ascending',
  DESCENDING: 'descending',
} as const

export type AuditSortAriaValue =
  (typeof AUDIT_SORT_ARIA_VALUES)[keyof typeof AUDIT_SORT_ARIA_VALUES]

export const AUDIT_SORT_INDICATOR_CLASSES = {
  ASCENDING: 'audit__sort-indicator--ascending',
  DESCENDING: 'audit__sort-indicator--descending',
} as const
