export const HTTP_METHOD = {
  GET: 'GET',
  POST: 'POST',
  PUT: 'PUT',
  PATCH: 'PATCH',
  DELETE: 'DELETE',
} as const

export const HTTP_HEADER = {
  CONTENT_TYPE: 'Content-Type',
} as const

export const HTTP_STATUS = {
  NO_CONTENT: 204,
  UNAUTHORIZED: 401,
} as const

export const MIME_TYPE = {
  JSON: 'application/json',
} as const
