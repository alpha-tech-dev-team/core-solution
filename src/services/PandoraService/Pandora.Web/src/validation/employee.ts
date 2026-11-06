import { z } from 'zod'
import { tr, type ResourceKey } from '@/i18n'
import { EMPLOYEE_FORM_MESSAGES } from '@/constants/i18n'
import { INPUT_LIMITS, INPUT_PATTERNS } from '@/constants/validation'
import type { EmployeeInput } from '@/stores/models/employee-input'

const personName = (message: string) =>
  z
    .string()
    .trim()
    .min(INPUT_LIMITS.PERSON_NAME_MIN, message)
    .max(INPUT_LIMITS.PERSON_NAME_MAX, message)
    .regex(INPUT_PATTERNS.PERSON_NAME, message)

export const employeeFormSchema = z.object({
  surname: personName(EMPLOYEE_FORM_MESSAGES.SURNAME),
  name: personName(EMPLOYEE_FORM_MESSAGES.NAME),
  patronymic: z
    .string()
    .trim()
    .max(INPUT_LIMITS.PERSON_NAME_MAX, EMPLOYEE_FORM_MESSAGES.PATRONYMIC)
    .refine(
      (value) => value === '' || INPUT_PATTERNS.PERSON_NAME.test(value),
      EMPLOYEE_FORM_MESSAGES.PATRONYMIC
    ),
  position: z
    .string()
    .trim()
    .min(INPUT_LIMITS.POSITION_MIN, EMPLOYEE_FORM_MESSAGES.POSITION)
    .max(INPUT_LIMITS.POSITION_MAX, EMPLOYEE_FORM_MESSAGES.POSITION)
    .regex(INPUT_PATTERNS.POSITION, EMPLOYEE_FORM_MESSAGES.POSITION),
  phone: z.string().trim().regex(INPUT_PATTERNS.PHONE, EMPLOYEE_FORM_MESSAGES.PHONE)
})

export type EmployeeFormField = keyof z.infer<typeof employeeFormSchema>

export type EmployeeFormErrors = Partial<Record<EmployeeFormField, string>>

export function validateEmployeeForm(input: EmployeeInput): EmployeeFormErrors {
  const result = employeeFormSchema.safeParse(input)
  if (result.success) return {}

  const errors: EmployeeFormErrors = {}
  for (const issue of result.error.issues) {
    const field = issue.path[0] as EmployeeFormField | undefined
    if (field !== undefined && errors[field] === undefined) errors[field] = tr(issue.message as ResourceKey)
  }
  return errors
}
