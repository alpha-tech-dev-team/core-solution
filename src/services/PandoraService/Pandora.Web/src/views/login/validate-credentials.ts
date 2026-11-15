import { trValidation } from '@/i18n'
import { VALIDATION_RULES } from '@/constants/i18n'
import { credentialsSchema, type Credentials } from '@/validation/credentials'
import { firstValidationMessage } from '@/validation/first-validation-message'

export function validateCredentials(credentials: Credentials): string | null {
  if (!credentials.username || !credentials.password) {
    return trValidation(VALIDATION_RULES.REQUIRED)
  }

  const result = credentialsSchema.safeParse(credentials)
  return result.success ? null : firstValidationMessage(result.error)
}
