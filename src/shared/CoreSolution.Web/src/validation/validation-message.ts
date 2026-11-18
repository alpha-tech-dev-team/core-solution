import type { z } from 'zod'
import { VALIDATION_RULES, type ValidationRule } from '../constants/validation'

export interface ValidationMessage {
  rule: ValidationRule | string
  limit?: unknown
}

export function firstValidationMessage(error: z.ZodError): ValidationMessage {
  const issue = error.issues[0]
  if (issue === undefined) return { rule: VALIDATION_RULES.REQUIRED }

  return { rule: issue.message, limit: extractLimit(issue) }
}

function extractLimit(issue: z.ZodError['issues'][number]): unknown {
  if (issue.code === 'too_small') return issue.minimum
  if (issue.code === 'too_big') return issue.maximum

  return undefined
}
