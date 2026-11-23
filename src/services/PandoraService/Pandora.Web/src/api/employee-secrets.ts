import { api } from '@shared/app/app-session'
import { EMPLOYEE_SECRET_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import type { EmployeeSecret } from '@/models/employee-secret'
import type { Page } from '@shared/models/page'

export async function fetchEmployeeSecrets(
  telegramUserId: number,
  page: number,
  pageSize: number,
): Promise<Page<EmployeeSecret>> {
  return await api.get<Page<EmployeeSecret>>(EMPLOYEE_SECRET_ENDPOINTS.LIST(telegramUserId), {
    [PAGINATION_QUERY.PAGE]: page,
    [PAGINATION_QUERY.PAGE_SIZE]: pageSize,
  })
}

export async function grantSecretAccess(telegramUserId: number, itemId: string): Promise<void> {
  await api.put(EMPLOYEE_SECRET_ENDPOINTS.BY_ID(telegramUserId, itemId))
}

export async function revokeSecretAccess(telegramUserId: number, itemId: string): Promise<void> {
  await api.delete(EMPLOYEE_SECRET_ENDPOINTS.BY_ID(telegramUserId, itemId))
}
