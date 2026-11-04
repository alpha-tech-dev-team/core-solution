import { z } from 'zod'
import { INPUT_LIMITS } from '@/constants/validation'
import { VALIDATION_RULES } from '@/constants/i18n'

export const secretFormSchema = z.object({
  name: z.string().max(INPUT_LIMITS.SECRET_NAME_MAX, VALIDATION_RULES.SECRET_NAME_TOO_LONG),
  username: z.string().max(INPUT_LIMITS.SECRET_USERNAME_MAX, VALIDATION_RULES.SECRET_USERNAME_TOO_LONG)
})

export type SecretForm = z.infer<typeof secretFormSchema>
