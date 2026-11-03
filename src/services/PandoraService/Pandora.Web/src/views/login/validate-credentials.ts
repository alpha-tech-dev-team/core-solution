import { z } from 'zod'
import { trValidation } from '@/i18n'
import { VALIDATION_RULES, type ValidationRule } from '@/constants/i18n'
import { ZOD_ISSUE_CODE } from '@/constants/validation'
import { credentialsSchema, type Credentials } from '@/validation/credentials'

type CredentialsIssue = z.ZodError['issues'][number]

function extractLimit(issue: CredentialsIssue): unknown {
  switch (issue.code) {
    case ZOD_ISSUE_CODE.TOO_SMALL:
      return issue.minimum
    case ZOD_ISSUE_CODE.TOO_BIG:
      return issue.maximum
    default:
      return undefined
  }
}

export function validateCredentials(credentials: Credentials): string | null {
  if (!credentials.username || !credentials.password) {
    return trValidation(VALIDATION_RULES.REQUIRED)
  }

  const result = credentialsSchema.safeParse(credentials)
  if (result.success) return null

  const issue = result.error.issues[0]
  return trValidation(issue.message as ValidationRule, extractLimit(issue))
}
