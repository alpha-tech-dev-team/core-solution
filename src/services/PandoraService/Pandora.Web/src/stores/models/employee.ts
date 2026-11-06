import type { EmployeeStatus } from './employee-status'

export interface Employee {
  id: string
  name: string
  surname: string
  patronymic: string | null
  position: string
  phone: string
  status: EmployeeStatus
  roles: string[]
}
