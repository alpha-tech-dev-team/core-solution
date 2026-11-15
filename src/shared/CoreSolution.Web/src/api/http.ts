export const HTTP_METHOD = {
  GET: 'GET',
  POST: 'POST',
  PUT: 'PUT',
  PATCH: 'PATCH',
  DELETE: 'DELETE',
} as const

export const HTTP_HEADER = {
  CONTENT_TYPE: 'Content-Type',
  AUTHORIZATION: 'Authorization',
} as const

export const HTTP_STATUS = {
  UNAUTHORIZED: 401,
} as const

export const MIME_TYPE = {
  JSON: 'application/json',
} as const
