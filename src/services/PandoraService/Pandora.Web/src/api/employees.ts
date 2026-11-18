import { api } from '@shared/app/app-session'
import { EMPLOYEE_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import { FIRST_PAGE } from '@shared/constants/pagination'
import { formatEmployeeFullName, type Employee } from '@/models/employee'
import type { EmployeeInput } from '@/models/employee-input'
import type { EmployeeRolesPayload } from '@/models/employee-roles-payload'
import type { EmployeeStatus } from '@/models/employee-status'
import type { Page } from '@shared/models/page'

const EMPLOYEE_NAME_REFRESH_INTERVAL_MS = 60_000

export interface EmployeeNames {
  byId: ReadonlyMap<string, string>
  byTelegramId: ReadonlyMap<number, string>
}

let employeeNamesCache: EmployeeNames | undefined
let employeeNamesRequest: Promise<EmployeeNames> | undefined
let lastEmployeeNamesRefresh = Number.NEGATIVE_INFINITY

export function fetchEmployeeNames(): Promise<EmployeeNames> {
  if (employeeNamesCache !== undefined) return Promise.resolve(employeeNamesCache)
  if (employeeNamesRequest !== undefined) return employeeNamesRequest

  return requestEmployeeNames()
}

export function refreshEmployeeNames(): Promise<EmployeeNames> {
  if (employeeNamesRequest !== undefined) return employeeNamesRequest
  if (Date.now() - lastEmployeeNamesRefresh < EMPLOYEE_NAME_REFRESH_INTERVAL_MS) {
    return fetchEmployeeNames()
  }

  return requestEmployeeNames(true)
}

function requestEmployeeNames(forceRefresh = false): Promise<EmployeeNames> {
  employeeNamesRequest = loadEmployeeNames()
    .then((names) => {
      employeeNamesCache = names
      if (forceRefresh) lastEmployeeNamesRefresh = Date.now()
      return names
    })
    .finally(() => {
      employeeNamesRequest = undefined
    })

  return employeeNamesRequest
}

async function loadEmployeeNames(): Promise<EmployeeNames> {
  const byId = new Map<string, string>()
  const byTelegramId = new Map<number, string>()

  for (let page = FIRST_PAGE; ; page += 1) {
    const result = await fetchEmployees(page, true)

    for (const employee of result.items) {
      const name = formatEmployeeFullName(employee)

      byId.set(employee.id, name)

      if (employee.telegramUserId !== null) {
        byTelegramId.set(employee.telegramUserId, name)
      }
    }

    if (result.items.length === 0 || result.page * result.pageSize >= result.total) {
      return { byId, byTelegramId }
    }
  }
}

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
