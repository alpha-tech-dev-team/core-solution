import { SHARED_RESOURCE_KEYS } from '@shared/constants/resource-keys'

export const RESOURCE_KEYS = {
  ...SHARED_RESOURCE_KEYS,

  ADMIN_SECTION_TITLE: 'admin_section_title',
  ADMIN_PAGE_TITLE: 'admin_page_title',
  ADMIN_PAGE_SUBTITLE: 'admin_page_subtitle',
  ADMIN_STATS_LABEL: 'admin_stats_label',
  ADMIN_USERS_LABEL: 'admin_users_label',
  ADMIN_SESSIONS_LABEL: 'admin_sessions_label',
  ADMIN_REQUESTS_LABEL: 'admin_requests_label',
  ADMIN_EMPTY_VALUE: 'admin_empty_value',
  ADMIN_RECENT_TITLE: 'admin_recent_title',
  ADMIN_EMPTY_STATE: 'admin_empty_state',
} as const
