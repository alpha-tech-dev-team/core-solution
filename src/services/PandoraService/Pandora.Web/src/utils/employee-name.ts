import type { Employee } from '@/stores/models/employee'

export function formatEmployeeName(employee: Employee): string {
  return [employee.surname, employee.name, employee.patronymic].filter(Boolean).join(' ')
}
