import { api } from '@shared/app/app-session'
import type { Page } from '@shared/models/page'
import { AUDIT_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import type { AuditEntry, AuditQuery } from '@/models/audit-entry'

export async function fetchAuditEntries(
  page: number,
  pageSize: number,
  query: AuditQuery = {},
): Promise<Page<AuditEntry>> {
  return await api.get<Page<AuditEntry>>(AUDIT_ENDPOINTS.COLLECTION, {
    [PAGINATION_QUERY.PAGE]: page,
    [PAGINATION_QUERY.PAGE_SIZE]: pageSize,
    ...query,
  })
}
