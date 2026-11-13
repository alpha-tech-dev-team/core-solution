import { z } from 'zod'
import { trValidation } from '@/i18n'
import { type ValidationRule } from '@/constants/i18n'
import { ZOD_ISSUE_CODE } from '@/constants/validation'
import { createSecretFormSchema, updateSecretFormSchema } from '@/validation/secrets'
import { type SecretFormValues } from '@/views/secrets/models/secret-form-values'

type SecretFormIssue = z.ZodError['issues'][number]

function extractLimit(issue: SecretFormIssue): unknown {
  switch (issue.code) {
    case ZOD_ISSUE_CODE.TOO_SMALL:
      return issue.minimum
    case ZOD_ISSUE_CODE.TOO_BIG:
      return issue.maximum
    default:
      return undefined
  }
}

export function validateSecretForm(values: SecretFormValues): string | null {
  const schema = values.isEditing ? updateSecretFormSchema : createSecretFormSchema
  const result = schema.safeParse({ name: values.name, username: values.username, password: values.password })
  if (result.success) return null

  const issue = result.error.issues[0]
  return trValidation(issue.message as ValidationRule, extractLimit(issue))
}
