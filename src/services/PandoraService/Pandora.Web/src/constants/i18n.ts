export const DEFAULT_LOCALE = 'ru'
export const SUPPORTED_LOCALES = ['ru', 'en'] as const

export type Locale = (typeof SUPPORTED_LOCALES)[number]

export const ERROR_KEY_PREFIX = 'error'
export const VALIDATION_KEY_PREFIX = 'validation'
export const PLACEHOLDER_PATTERN = /\{(\d+)\}/g

export const VALIDATION_RULES = {
  REQUIRED: 'required',
  USERNAME_TOO_SHORT: 'usernameTooShort',
  USERNAME_TOO_LONG: 'usernameTooLong',
  PASSWORD_TOO_SHORT: 'passwordTooShort',
  PASSWORD_TOO_LONG: 'passwordTooLong'
} as const

export type ValidationRule = (typeof VALIDATION_RULES)[keyof typeof VALIDATION_RULES]

import type { ResourceKey } from '@/i18n'

export const EMPLOYEE_FORM_MESSAGES = {
  NAME: 'employee_error_name',
  SURNAME: 'employee_error_surname',
  PATRONYMIC: 'employee_error_patronymic',
  POSITION: 'employee_error_position',
  PHONE: 'employee_error_phone'
} as const satisfies Record<string, ResourceKey>
