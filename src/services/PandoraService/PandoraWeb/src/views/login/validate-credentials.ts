import { INPUT_LIMITS } from './constants'
import { trValidation } from '@/i18n'
import { VALIDATION_RULES } from '@/i18n/keys'

export interface Credentials {
  username: string
  password: string
}

export function validateCredentials({ username, password }: Credentials): string | null {
  if (!username || !password) {
    return trValidation(VALIDATION_RULES.REQUIRED)
  }
  if (username.length < INPUT_LIMITS.USERNAME_MIN) {
    return trValidation(VALIDATION_RULES.USERNAME_TOO_SHORT, INPUT_LIMITS.USERNAME_MIN)
  }
  if (username.length > INPUT_LIMITS.USERNAME_MAX) {
    return trValidation(VALIDATION_RULES.USERNAME_TOO_LONG, INPUT_LIMITS.USERNAME_MAX)
  }
  if (password.length < INPUT_LIMITS.PASSWORD_MIN) {
    return trValidation(VALIDATION_RULES.PASSWORD_TOO_SHORT, INPUT_LIMITS.PASSWORD_MIN)
  }
  if (password.length > INPUT_LIMITS.PASSWORD_MAX) {
    return trValidation(VALIDATION_RULES.PASSWORD_TOO_LONG, INPUT_LIMITS.PASSWORD_MAX)
  }
  return null
}