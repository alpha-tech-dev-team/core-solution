import { api } from '@/session'
import { EMPLOYEE_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import type { EmployeeInput } from '@/models/employee-input'
import type { EmployeeRolesPayload } from '@/models/employee-roles-payload'
import type { EmployeeStatus } from '@/models/employee-status'
import type { EmployeesPage } from '@/models/employees-page'

export async function fetchEmployees(page: number): Promise<EmployeesPage> {
  return await api.get<EmployeesPage>(EMPLOYEE_ENDPOINTS.LIST, {
    [PAGINATION_QUERY.PAGE]: page,
  })
}

export async function fetchEmployeeRoles(): Promise<string[]> {
  return await api.get<string[]>(EMPLOYEE_ENDPOINTS.ROLES)
}

export async function createEmployee(input: EmployeeInput): Promise<void> {
  await api.post(EMPLOYEE_ENDPOINTS.LIST, input)
}

export async function updateEmployee(employeeId: string, input: EmployeeInput): Promise<void> {
  await api.put(EMPLOYEE_ENDPOINTS.BY_ID(employeeId), input)
}

export async function deleteEmployee(employeeId: string): Promise<void> {
  await api.delete(EMPLOYEE_ENDPOINTS.BY_ID(employeeId))
}

export async function assignEmployeeRoles(
  employeeId: string,
  payload: EmployeeRolesPayload,
): Promise<void> {
  await api.put(EMPLOYEE_ENDPOINTS.ROLES_OF(employeeId), payload)
}

export async function updateEmployeeStatus(
  employeeId: string,
  status: EmployeeStatus,
): Promise<void> {
  await api.put(EMPLOYEE_ENDPOINTS.STATUS_OF(employeeId), { status })
}
