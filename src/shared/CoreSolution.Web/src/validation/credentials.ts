import { z } from 'zod'
import { CREDENTIALS_LIMITS, VALIDATION_RULES } from '../constants/validation'
import type { Credentials } from '../models/credentials'

export const credentialsSchema: z.ZodType<Credentials> = z.object({
  username: z
    .string()
    .trim()
    .min(CREDENTIALS_LIMITS.USERNAME_MIN_LENGTH, VALIDATION_RULES.REQUIRED)
    .max(CREDENTIALS_LIMITS.USERNAME_MAX_LENGTH, VALIDATION_RULES.USERNAME_TOO_LONG),
  password: z
    .string()
    .min(CREDENTIALS_LIMITS.PASSWORD_MIN_LENGTH, VALIDATION_RULES.REQUIRED)
    .max(CREDENTIALS_LIMITS.PASSWORD_MAX_LENGTH, VALIDATION_RULES.PASSWORD_TOO_LONG),
})
