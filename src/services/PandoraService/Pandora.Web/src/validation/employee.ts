import { z } from 'zod'
import { INPUT_LIMITS, INPUT_PATTERNS } from '@/constants/validation'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import type { EmployeeInput } from '@/models/employee-input'

const personName = (message: string) =>
  z
    .string()
    .trim()
    .min(INPUT_LIMITS.PERSON_NAME_MIN, message)
    .max(INPUT_LIMITS.PERSON_NAME_MAX, message)
    .regex(INPUT_PATTERNS.PERSON_NAME, message)

export const employeeFormSchema = z.object({
  surname: personName(RESOURCE_KEYS.EMPLOYEE_ERROR_SURNAME),
  name: personName(RESOURCE_KEYS.EMPLOYEE_ERROR_NAME),
  patronymic: z
    .string()
    .trim()
    .max(INPUT_LIMITS.PERSON_NAME_MAX, RESOURCE_KEYS.EMPLOYEE_ERROR_PATRONYMIC)
    .refine(
      (value) => value === '' || INPUT_PATTERNS.PERSON_NAME.test(value),
      RESOURCE_KEYS.EMPLOYEE_ERROR_PATRONYMIC,
    ),
  position: z
    .string()
    .trim()
    .min(INPUT_LIMITS.POSITION_MIN, RESOURCE_KEYS.EMPLOYEE_ERROR_POSITION)
    .max(INPUT_LIMITS.POSITION_MAX, RESOURCE_KEYS.EMPLOYEE_ERROR_POSITION)
    .regex(INPUT_PATTERNS.POSITION, RESOURCE_KEYS.EMPLOYEE_ERROR_POSITION),
  phone: z
    .string()
    .trim()
    .max(INPUT_LIMITS.PHONE_MAX_LENGTH, RESOURCE_KEYS.EMPLOYEE_ERROR_PHONE)
    .regex(INPUT_PATTERNS.PHONE, RESOURCE_KEYS.EMPLOYEE_ERROR_PHONE),
})

export type EmployeeFormField = keyof z.infer<typeof employeeFormSchema>

export type EmployeeFormErrors = Partial<Record<EmployeeFormField, string>>

export function validateEmployeeForm(input: EmployeeInput): EmployeeFormErrors {
  const result = employeeFormSchema.safeParse(input)
  if (result.success) return {}

  const errors: EmployeeFormErrors = {}
  for (const issue of result.error.issues) {
    const field = issue.path[0] as EmployeeFormField | undefined
    if (field !== undefined && errors[field] === undefined) {
      errors[field] = issue.message
    }
  }

  return errors
}
