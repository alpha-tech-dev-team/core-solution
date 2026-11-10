export const DEFAULT_LOCALE = 'ru'
export const SUPPORTED_LOCALES = ['ru', 'en'] as const

export type Locale = (typeof SUPPORTED_LOCALES)[number]

export const ERROR_KEY_PREFIX = 'error'
export const VALIDATION_KEY_PREFIX = 'validation'
export const AUDIT_ACTION_KEY_PREFIX = 'audit_action'
export const AUDIT_SOURCE_KEY_PREFIX = 'audit_source'
export const PLACEHOLDER_PATTERN = /\{(\d+)\}/g

export const RESOURCE_KEYS = {
  APP_TITLE: 'app_title',
  APP_PLACEHOLDER: 'app_placeholder',
  AUTH_LOGIN: 'auth_login',
  AUTH_LOGOUT: 'auth_logout',
  FIELD_USERNAME: 'login_field_username',
  FIELD_PASSWORD: 'login_field_password',
  NAV_AUDIT: 'nav_audit',
  AUDIT_TITLE: 'audit_title',
  AUDIT_COLUMN_TIME: 'audit_column_time',
  AUDIT_COLUMN_USER: 'audit_column_user',
  AUDIT_COLUMN_ACTION: 'audit_column_action',
  AUDIT_COLUMN_TARGET: 'audit_column_target',
  AUDIT_COLUMN_SOURCE: 'audit_column_source',
  AUDIT_EMPTY: 'audit_empty',
  AUDIT_PAGE_INFO: 'audit_page_info',
  AUDIT_PREV_PAGE: 'audit_prev_page',
  AUDIT_NEXT_PAGE: 'audit_next_page'
} as const

export const VALIDATION_RULES = {
  REQUIRED: 'required',
  USERNAME_TOO_SHORT: 'usernameTooShort',
  USERNAME_TOO_LONG: 'usernameTooLong',
  PASSWORD_TOO_SHORT: 'passwordTooShort',
  PASSWORD_TOO_LONG: 'passwordTooLong'
} as const

export const AUDIT_ACTIONS = {
  show_services: `${AUDIT_ACTION_KEY_PREFIX}_show_services`,
  get_credentials: `${AUDIT_ACTION_KEY_PREFIX}_get_credentials`
} as const

export const AUDIT_SOURCES = {
  telegram: `${AUDIT_SOURCE_KEY_PREFIX}_telegram`
} as const

export type AuditAction = keyof typeof AUDIT_ACTIONS
export type AuditSource = keyof typeof AUDIT_SOURCES

export const SERVER_ERROR_KEYS: Record<string, string> = {
  AccessDenied: 'error_code_accessDenied',
  AccessNotConfirmed: 'error_code_accessNotConfirmed',
  SecretNotFound: 'error_code_secretNotFound',
  VaultwardenUnavailable: 'error_code_vaultwardenUnavailable',
  InternalError: 'error_code_internalError'
}

export type ValidationRule = (typeof VALIDATION_RULES)[keyof typeof VALIDATION_RULES]