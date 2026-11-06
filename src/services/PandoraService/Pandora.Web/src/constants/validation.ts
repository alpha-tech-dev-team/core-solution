export const INPUT_LIMITS = {
  USERNAME_MIN: 3,
  USERNAME_MAX: 50,
  PASSWORD_MIN: 6,
  PASSWORD_MAX: 128,
  SECRET_NAME_MAX: 100,
  SECRET_USERNAME_MAX: 100
} as const

export const ZOD_ISSUE_CODE = {
  TOO_SMALL: 'too_small',
  TOO_BIG: 'too_big'
} as const