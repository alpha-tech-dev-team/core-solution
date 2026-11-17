import api from './client'
import { AUDIT_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import type { AuditEntryPage, AuditQuery } from '@/stores/models/audit-entry'

export async function fetchAuditEntries(page: number, pageSize: number, query: AuditQuery = {}): Promise<AuditEntryPage> {
  const { data } = await api.get<AuditEntryPage>(AUDIT_ENDPOINTS.COLLECTION, {
    params: {
      [PAGINATION_QUERY.PAGE]: page,
      [PAGINATION_QUERY.PAGE_SIZE]: pageSize,
      ...query
    }
  })
  return data
}
