import { z } from 'zod'
import { LOGIN_VALIDATION } from '@/constants/validation'
import { ru } from '@/i18n'

export const loginSchema = z.object({
  username: z
    .string()
    .trim()
    .min(LOGIN_VALIDATION.USERNAME_MIN_LENGTH, ru.auth.errorUsernameRequired)
    .max(LOGIN_VALIDATION.USERNAME_MAX_LENGTH, ru.auth.errorUsernameTooLong),
  password: z
    .string()
    .min(LOGIN_VALIDATION.PASSWORD_MIN_LENGTH, ru.auth.errorPasswordRequired)
    .max(LOGIN_VALIDATION.PASSWORD_MAX_LENGTH, ru.auth.errorPasswordTooLong),
})
