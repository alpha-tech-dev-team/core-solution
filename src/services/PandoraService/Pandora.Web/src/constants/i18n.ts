import type { ResourceKey } from '@/i18n'

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
  AUDIT_NEXT_PAGE: 'audit_next_page',
  NAV_HOME: 'nav_home',
  NAV_SECRETS: 'nav_secrets',
  SECRETS_TITLE: 'secrets_title',
  SECRETS_ADD: 'secrets_add',
  SECRETS_SAVE: 'secrets_save',
  SECRETS_CANCEL: 'secrets_cancel',
  SECRETS_EDIT: 'secrets_edit',
  SECRETS_DELETE: 'secrets_delete',
  SECRETS_DELETE_CONFIRM: 'secrets_delete_confirm',
  SECRETS_EMPTY: 'secrets_empty',
  SECRETS_FIELD_NAME: 'secrets_field_name',
  SECRETS_FIELD_USERNAME: 'secrets_field_username',
  SECRETS_FIELD_PASSWORD: 'secrets_field_password',
  SECRETS_PASSWORD_KEEP: 'secrets_password_keep',
  SECRETS_PAGE_PREV: 'secrets_page_prev',
  SECRETS_PAGE_NEXT: 'secrets_page_next',
  SECRETS_PAGE_INFO: 'secrets_page_info'
} as const

export const VALIDATION_RULES = {
  REQUIRED: 'required',
  USERNAME_TOO_SHORT: 'usernameTooShort',
  USERNAME_TOO_LONG: 'usernameTooLong',
  PASSWORD_TOO_SHORT: 'passwordTooShort',
  PASSWORD_TOO_LONG: 'passwordTooLong',
  SECRET_NAME_TOO_LONG: 'secretNameTooLong',
  SECRET_USERNAME_TOO_LONG: 'secretUsernameTooLong'
} as const

export const AUDIT_ACTIONS = {
  show_services: `${AUDIT_ACTION_KEY_PREFIX}_show_services`,
  get_credentials: `${AUDIT_ACTION_KEY_PREFIX}_get_credentials`,
  secret_created: `${AUDIT_ACTION_KEY_PREFIX}_secret_created`,
  secret_updated: `${AUDIT_ACTION_KEY_PREFIX}_secret_updated`,
  secret_deleted: `${AUDIT_ACTION_KEY_PREFIX}_secret_deleted`
} as const

export const AUDIT_SOURCES = {
  telegram: `${AUDIT_SOURCE_KEY_PREFIX}_telegram`,
  web: `${AUDIT_SOURCE_KEY_PREFIX}_web`
} as const

export type AuditAction = keyof typeof AUDIT_ACTIONS
export type AuditSource = keyof typeof AUDIT_SOURCES

export type ValidationRule = (typeof VALIDATION_RULES)[keyof typeof VALIDATION_RULES]

export const EMPLOYEE_FORM_MESSAGES = {
  NAME: 'employee_error_name',
  SURNAME: 'employee_error_surname',
  PATRONYMIC: 'employee_error_patronymic',
  POSITION: 'employee_error_position',
  PHONE: 'employee_error_phone'
} as const satisfies Record<string, ResourceKey>

export const SERVER_ERROR_KEYS: Record<string, string> = {
  AccessDenied: 'error_code_accessDenied',
  AccessNotConfirmed: 'error_code_accessNotConfirmed',
  SecretNotFound: 'error_code_secretNotFound',
  InvalidSecretData: 'error_code_invalidSecretData',
  VaultwardenUnavailable: 'error_code_vaultwardenUnavailable',
  DatabaseUnavailable: 'error_code_databaseUnavailable',
  InternalError: 'error_code_internalError',
  BadRequest: 'error_code_badRequest'
}
