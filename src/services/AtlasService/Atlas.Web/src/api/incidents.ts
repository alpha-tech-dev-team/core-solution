import { api } from '@shared/app/app-session'
import { INCIDENT_ENDPOINTS, PAGINATION_QUERY } from '@/constants/api'
import type { IncidentItem } from '@/models/incident-item'
import type { Page } from '@shared/models/page'

export async function fetchIncidents(
  page: number,
  pageSize: number,
): Promise<Page<IncidentItem>> {
  return await api.get<Page<IncidentItem>>(INCIDENT_ENDPOINTS.COLLECTION, {
    [PAGINATION_QUERY.PAGE]: page,
    [PAGINATION_QUERY.PAGE_SIZE]: pageSize,
  })
}
