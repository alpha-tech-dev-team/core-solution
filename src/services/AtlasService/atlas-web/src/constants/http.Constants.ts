export const HttpMethod = {
  GET: 'GET',
  POST: 'POST',
  PUT: 'PUT',
  DELETE: 'DELETE',
  PATCH: 'PATCH',
} as const

export const HttpHeader = {
  ContentType: 'Content-Type',
  Authorization: 'Authorization',
} as const

export const HttpStatus = {
  Unauthorized: 401,
} as const

export const MimeType = {
  Json: 'application/json',
} as const
