import api from './client'
import { EMPLOYEE_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import { EMPLOYEES_PAGE_SIZE, FIRST_PAGE } from '@/constants/pagination'
import { formatEmployeeName } from '@/utils/employee-name'
import type { EmployeesPage } from '@/stores/models/employees-page'

export interface EmployeeNames {
  byId: ReadonlyMap<string, string>
  byTelegramId: ReadonlyMap<number, string>
}

export async function fetchEmployeeNames(): Promise<EmployeeNames> {
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

    if (page * EMPLOYEES_PAGE_SIZE >= data.total) {
      return { byId, byTelegramId }
    }
  }
}
