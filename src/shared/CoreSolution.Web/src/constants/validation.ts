export const CREDENTIALS_LIMITS = {
  USERNAME_MIN_LENGTH: 1,
  USERNAME_MAX_LENGTH: 50,
  PASSWORD_MIN_LENGTH: 1,
  PASSWORD_MAX_LENGTH: 128,
} as const

export const VALIDATION_RULES = {
  REQUIRED: 'required',
  USERNAME_TOO_LONG: 'usernameTooLong',
  PASSWORD_TOO_LONG: 'passwordTooLong',
} as const

export type ValidationRule = (typeof VALIDATION_RULES)[keyof typeof VALIDATION_RULES]
