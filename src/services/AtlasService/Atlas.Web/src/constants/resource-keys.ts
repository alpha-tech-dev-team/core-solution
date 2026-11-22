import { SHARED_RESOURCE_KEYS } from '@shared/constants/resource-keys'

export const RESOURCE_KEYS = {
  ...SHARED_RESOURCE_KEYS,

  NAV_INCIDENTS: 'nav_incidents',

  INCIDENTS_TITLE: 'incidents_title',
  INCIDENTS_EMPTY: 'incidents_empty',
  INCIDENTS_PAGE_PREV: 'incidents_page_prev',
  INCIDENTS_PAGE_NEXT: 'incidents_page_next',
  INCIDENTS_PAGE_INFO: 'incidents_page_info',

  INCIDENT_NUMBER: 'incident_number',
  INCIDENT_CREATED_AT: 'incident_created_at',
  INCIDENT_EMPLOYEE: 'incident_employee',
  INCIDENT_CATEGORY: 'incident_category',
  INCIDENT_ROOM: 'incident_room',
  INCIDENT_ASSET: 'incident_asset',
  INCIDENT_DESCRIPTION: 'incident_description',
  INCIDENT_STATUS: 'incident_status',
  INCIDENT_PRIORITY: 'incident_priority',
  INCIDENT_ASSIGNED_ENGINEER: 'incident_assigned_engineer',
  INCIDENT_NO_ASSIGNEE: 'incident_no_assignee',
  INCIDENT_NO_ASSET: 'incident_no_asset',
  INCIDENT_NO_ROOM: 'incident_no_room',

} as const
