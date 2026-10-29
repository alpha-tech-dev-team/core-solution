export const ERROR_KEY_PREFIX = 'error'
export const VALIDATION_KEY_PREFIX = 'validation'

export const RESOURCE_KEYS = {
  APP_TITLE: 'app_title',
  APP_PLACEHOLDER: 'app_placeholder',
  AUTH_LOGIN: 'auth_login',
  AUTH_LOGOUT: 'auth_logout',
  FIELD_USERNAME: 'login_field_username',
  FIELD_PASSWORD: 'login_field_password'
} as const

export const VALIDATION_RULES = {
  REQUIRED: 'required',
  USERNAME_TOO_SHORT: 'usernameTooShort',
  USERNAME_TOO_LONG: 'usernameTooLong',
  PASSWORD_TOO_SHORT: 'passwordTooShort',
  PASSWORD_TOO_LONG: 'passwordTooLong'
} as const

export type ValidationRule = (typeof VALIDATION_RULES)[keyof typeof VALIDATION_RULES]