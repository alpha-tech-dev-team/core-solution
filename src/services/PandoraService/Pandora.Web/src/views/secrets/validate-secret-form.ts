import { createSecretFormSchema, updateSecretFormSchema } from '@/validation/secrets'
import { firstValidationMessage } from '@shared/validation/validation-message'
import type { ValidationMessage } from '@shared/validation/validation-message'

export interface SecretFormValues {
  name: string
  username: string
  password: string
  isEditing: boolean
}

export function validateSecretForm(values: SecretFormValues): ValidationMessage | null {
  const schema = values.isEditing ? updateSecretFormSchema : createSecretFormSchema

  const result = schema.safeParse({
    name: values.name,
    username: values.username,
    password: values.password,
  })

  return result.success ? null : firstValidationMessage(result.error)
}
