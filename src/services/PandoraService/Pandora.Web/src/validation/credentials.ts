import { z } from 'zod'
import { INPUT_LIMITS } from '@/constants/validation'
import { VALIDATION_RULES } from '@/constants/i18n'

export const credentialsSchema = z.object({
  username: z
    .string()
    .min(INPUT_LIMITS.USERNAME_MIN, VALIDATION_RULES.USERNAME_TOO_SHORT)
    .max(INPUT_LIMITS.USERNAME_MAX, VALIDATION_RULES.USERNAME_TOO_LONG),
  password: z
    .string()
    .min(INPUT_LIMITS.PASSWORD_MIN, VALIDATION_RULES.PASSWORD_TOO_SHORT)
    .max(INPUT_LIMITS.PASSWORD_MAX, VALIDATION_RULES.PASSWORD_TOO_LONG)
})

export type Credentials = z.infer<typeof credentialsSchema>
