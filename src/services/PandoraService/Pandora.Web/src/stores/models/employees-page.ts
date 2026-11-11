import type { Employee } from './employee'

export interface EmployeesPage {
  items: Employee[]
  total: number
  page: number
  pageSize: number
}
