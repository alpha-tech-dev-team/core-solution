import { z } from 'zod'
import { CREDENTIALS_LIMITS, VALIDATION_RULES } from '@shared/constants/validation'
import { INPUT_LIMITS, VALIDATION_RULES as APP_VALIDATION_RULES } from '@/constants/validation'

export const secretFormSchema = z.object({
  name: z
    .string()
    .min(1, VALIDATION_RULES.REQUIRED)
    .max(INPUT_LIMITS.SECRET_NAME_MAX, APP_VALIDATION_RULES.SECRET_NAME_TOO_LONG),
  username: z
    .string()
    .min(1, VALIDATION_RULES.REQUIRED)
    .max(INPUT_LIMITS.SECRET_USERNAME_MAX, APP_VALIDATION_RULES.SECRET_USERNAME_TOO_LONG),
})

export const createSecretFormSchema = secretFormSchema.extend({
  password: z
    .string()
    .min(1, VALIDATION_RULES.REQUIRED)
    .max(CREDENTIALS_LIMITS.PASSWORD_MAX_LENGTH, VALIDATION_RULES.PASSWORD_TOO_LONG),
})

export const updateSecretFormSchema = secretFormSchema.extend({
  password: z
    .string()
    .max(CREDENTIALS_LIMITS.PASSWORD_MAX_LENGTH, VALIDATION_RULES.PASSWORD_TOO_LONG),
})

export type SecretForm = z.infer<typeof secretFormSchema>
