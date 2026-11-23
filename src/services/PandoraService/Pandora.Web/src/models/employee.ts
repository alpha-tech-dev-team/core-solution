import type { EmployeeStatus } from '@/models/employee-status'

export interface Employee {
  id: string
  name: string
  surname: string
  patronymic: string | null
  position: string
  phone: string
  status: EmployeeStatus
  telegramUserId: number | null
  roles: string[]
  adminUsername: string | null
}

export function formatEmployeeFullName(employee: Employee): string {
  return [employee.surname, employee.name, employee.patronymic].filter(Boolean).join(' ')
}
