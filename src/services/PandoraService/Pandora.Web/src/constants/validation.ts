export const INPUT_LIMITS = {
  USERNAME_MIN: 3,
  USERNAME_MAX: 50,
  PASSWORD_MIN: 6,
  PASSWORD_MAX: 128,
  PERSON_NAME_MIN: 2,
  PERSON_NAME_MAX: 50,
  POSITION_MIN: 2,
  POSITION_MAX: 50,
  PHONE_DIGITS_MIN: 10,
  PHONE_DIGITS_MAX: 15
} as const

export const INPUT_PATTERNS = {
  PERSON_NAME: /^[A-Za-zА-Яа-яЁё][A-Za-zА-Яа-яЁё \-]*$/,
  POSITION: /^[A-Za-zА-Яа-яЁё0-9][A-Za-zА-Яа-яЁё0-9 \-().]*$/,
  PHONE: new RegExp(`^\\+[0-9]{${INPUT_LIMITS.PHONE_DIGITS_MIN},${INPUT_LIMITS.PHONE_DIGITS_MAX}}$`)
} as const

export const ZOD_ISSUE_CODE = {
  TOO_SMALL: 'too_small',
  TOO_BIG: 'too_big'
} as const
