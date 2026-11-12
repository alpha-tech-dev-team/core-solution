export const EMPLOYEE_STATUS = {
  PENDING: 'Pending',
  ACTIVE: 'Active',
  BLOCKED: 'Blocked'
} as const

export type EmployeeStatus = (typeof EMPLOYEE_STATUS)[keyof typeof EMPLOYEE_STATUS]
