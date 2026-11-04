import { z } from 'zod'
import { trValidation } from '@/i18n'
import { VALIDATION_RULES, type ValidationRule } from '@/constants/i18n'
import { ZOD_ISSUE_CODE } from '@/constants/validation'
import { secretFormSchema, type SecretForm } from '@/validation/secrets'

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

export interface SecretFormValues extends SecretForm {
  password: string
  isEditing: boolean
}

export function validateSecretForm(values: SecretFormValues): string | null {
  if (!values.name || !values.username || (!values.isEditing && !values.password)) {
    return trValidation(VALIDATION_RULES.REQUIRED)
  }

  const result = secretFormSchema.safeParse({ name: values.name, username: values.username })
  if (result.success) return null

  const issue = result.error.issues[0]
  return trValidation(issue.message as ValidationRule, extractLimit(issue))
}
