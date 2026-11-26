export const ROOM_ENDPOINTS = {
  COLLECTION: '/rooms',
  BY_ID: (id: string) => `/rooms/${id}`,
} as const

export const INCIDENT_ENDPOINTS = {
  COLLECTION: '/incidents',
} as const

export const PAGINATION_QUERY = {
  PAGE: 'page',
  PAGE_SIZE: 'pageSize',
} as const
