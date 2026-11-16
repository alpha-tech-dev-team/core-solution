import { z } from 'zod'
import { INPUT_LIMITS } from '@/constants/validation'
import { VALIDATION_RULES } from '@/constants/i18n'

export const secretFormSchema = z.object({
  name: z.string()
    .min(1, VALIDATION_RULES.REQUIRED)
    .max(INPUT_LIMITS.SECRET_NAME_MAX, VALIDATION_RULES.SECRET_NAME_TOO_LONG),
  username: z.string()
    .min(1, VALIDATION_RULES.REQUIRED)
    .max(INPUT_LIMITS.SECRET_USERNAME_MAX, VALIDATION_RULES.SECRET_USERNAME_TOO_LONG)
})

export const createSecretFormSchema = secretFormSchema.extend({
  password: z.string()
    .min(1, VALIDATION_RULES.REQUIRED)
    .max(INPUT_LIMITS.PASSWORD_MAX, VALIDATION_RULES.PASSWORD_TOO_LONG)
})

export const updateSecretFormSchema = secretFormSchema.extend({
  password: z.string()
    .max(INPUT_LIMITS.PASSWORD_MAX, VALIDATION_RULES.PASSWORD_TOO_LONG)
})

export type SecretForm = z.infer<typeof secretFormSchema>
