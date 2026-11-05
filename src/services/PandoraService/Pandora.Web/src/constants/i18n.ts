import { EMPLOYEE_STATUS } from '@/stores/models/employee-status'

export const DEFAULT_LOCALE = 'ru'
export const SUPPORTED_LOCALES = ['ru', 'en'] as const

export type Locale = (typeof SUPPORTED_LOCALES)[number]

export const ERROR_KEY_PREFIX = 'error'
export const VALIDATION_KEY_PREFIX = 'validation'
export const PLACEHOLDER_PATTERN = /\{(\d+)\}/g

export const RESOURCE_KEYS = {
  APP_TITLE: 'app_title',
  AUTH_LOGIN: 'auth_login',
  AUTH_LOGOUT: 'auth_logout',
  FIELD_USERNAME: 'login_field_username',
  FIELD_PASSWORD: 'login_field_password',
  EMPLOYEES_TITLE: 'employees_title',
  EMPLOYEE_NAME: 'employee_name',
  EMPLOYEE_POSITION: 'employee_position',
  EMPLOYEE_PHONE: 'employee_phone',
  EMPLOYEE_STATUS: 'employee_status',
  EMPLOYEE_ROLES: 'employee_roles',
  EMPLOYEE_BLOCK: 'employee_block',
  EMPLOYEE_UNBLOCK: 'employee_unblock',
  EMPLOYEES_EMPTY: 'employees_empty',
  STATUS_PENDING: 'status_pending',
  STATUS_ACTIVE: 'status_active',
  STATUS_BLOCKED: 'status_blocked'
} as const

export const EMPLOYEE_STATUS_RESOURCE_KEYS: Record<string, string> = {
  [EMPLOYEE_STATUS.PENDING]: RESOURCE_KEYS.STATUS_PENDING,
  [EMPLOYEE_STATUS.ACTIVE]: RESOURCE_KEYS.STATUS_ACTIVE,
  [EMPLOYEE_STATUS.BLOCKED]: RESOURCE_KEYS.STATUS_BLOCKED
}

export const VALIDATION_RULES = {
  REQUIRED: 'required',
  USERNAME_TOO_SHORT: 'usernameTooShort',
  USERNAME_TOO_LONG: 'usernameTooLong',
  PASSWORD_TOO_SHORT: 'passwordTooShort',
  PASSWORD_TOO_LONG: 'passwordTooLong'
} as const

export type ValidationRule = (typeof VALIDATION_RULES)[keyof typeof VALIDATION_RULES]