import { SHARED_RESOURCE_KEYS } from '@shared/constants/resource-keys'

export const RESOURCE_KEYS = {
  ...SHARED_RESOURCE_KEYS,

  NAV_INCIDENTS: 'nav_incidents',
  NAV_ROOMS: 'nav_rooms',

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

  ROOMS_TITLE: 'rooms_title',
  ROOMS_EMPTY: 'rooms_empty',
  ROOMS_PAGE_PREV: 'rooms_page_prev',
  ROOMS_PAGE_NEXT: 'rooms_page_next',
  ROOMS_PAGE_INFO: 'rooms_page_info',

  ROOM_NAME: 'room_name',
  ROOM_DESCRIPTION: 'room_description',
  ROOM_CREATED_AT: 'room_created_at',
  ROOM_NO_DESCRIPTION: 'room_no_description',
  ROOM_ADD: 'room_add',
  ROOM_EDIT: 'room_edit',
  ROOM_DELETE: 'room_delete',
  ROOM_DELETE_CONFIRM: 'room_delete_confirm',
  ROOM_SAVE: 'room_save',
  ROOM_CANCEL: 'room_cancel',
} as const
