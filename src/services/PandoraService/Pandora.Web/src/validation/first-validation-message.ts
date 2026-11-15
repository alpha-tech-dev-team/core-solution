import type { z } from 'zod'
import { trValidation } from '@/i18n'
import { VALIDATION_RULES, type ValidationRule } from '@/constants/i18n'
import { ZOD_ISSUE_CODE } from '@/constants/validation'

type ZodIssue = z.ZodError['issues'][number]

function extractLimit(issue: ZodIssue): unknown {
  switch (issue.code) {
    case ZOD_ISSUE_CODE.TOO_SMALL:
      return issue.minimum
    case ZOD_ISSUE_CODE.TOO_BIG:
      return issue.maximum
    default:
      return undefined
  }
}

export function firstValidationMessage(error: z.ZodError): string {
  const issue = error.issues[0]
  if (issue === undefined) return trValidation(VALIDATION_RULES.REQUIRED)

  return trValidation(issue.message as ValidationRule, extractLimit(issue))
}
