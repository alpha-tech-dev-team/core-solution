import { createSecretFormSchema, updateSecretFormSchema } from '@/validation/secrets'
import { firstValidationMessage } from '@/validation/first-validation-message'
import type { SecretFormValues } from '@/models/secret-form-values'

export function validateSecretForm(values: SecretFormValues): string | null {
  const schema = values.isEditing ? updateSecretFormSchema : createSecretFormSchema

  const result = schema.safeParse({
    name: values.name,
    username: values.username,
    password: values.password,
  })

  return result.success ? null : firstValidationMessage(result.error)
}
