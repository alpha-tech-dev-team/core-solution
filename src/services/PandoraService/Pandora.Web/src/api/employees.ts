import api from './client'
import { EMPLOYEE_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import { EMPLOYEES_PAGE_SIZE, FIRST_PAGE } from '@/constants/pagination'
import { formatEmployeeName } from '@/utils/employee-name'
import type { EmployeesPage } from '@/stores/models/employees-page'

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
  if (Date.now() - lastEmployeeNamesRefresh < EMPLOYEE_NAME_REFRESH_INTERVAL_MS) return fetchEmployeeNames()

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
    const { data } = await api.get<EmployeesPage>(EMPLOYEE_ENDPOINTS.LIST, {
      params: {
        [PAGINATION_QUERY.PAGE]: page,
        [PAGINATION_QUERY.PAGE_SIZE]: EMPLOYEES_PAGE_SIZE,
        includeArchived: true
      }
    })

    for (const employee of data.items) {
      const name = formatEmployeeName(employee)

      byId.set(employee.id, name)

      if (employee.telegramUserId !== null) {
        byTelegramId.set(employee.telegramUserId, name)
      }
    }

    if (data.items.length === 0 || data.page * data.pageSize >= data.total) {
      return { byId, byTelegramId }
    }
  }
}
