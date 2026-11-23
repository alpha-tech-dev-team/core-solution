import { api } from '@shared/app/app-session'
import { EMPLOYEE_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import type { Employee } from '@/models/employee'
import type { EmployeeInput } from '@/models/employee-input'
import type { EmployeeRolesPayload } from '@/models/employee-roles-payload'
import type { EmployeeStatus } from '@/models/employee-status'
import type { Page } from '@shared/models/page'

export async function fetchEmployees(
  page: number,
  includeArchived: boolean,
): Promise<Page<Employee>> {
  return await api.get<Page<Employee>>(EMPLOYEE_ENDPOINTS.LIST, {
    [PAGINATION_QUERY.PAGE]: page,
    [PAGINATION_QUERY.INCLUDE_ARCHIVED]: includeArchived,
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
