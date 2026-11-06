import { AxiosError } from 'axios'
import { REQUEST_ERROR } from '@/constants/api'
import { apiErrorSchema } from '@/validation/api-error'

export type RequestError = string

export function toRequestError(error: unknown): RequestError {
  if (!(error instanceof AxiosError)) return REQUEST_ERROR.UNKNOWN
  if (!error.response) return REQUEST_ERROR.NETWORK

  const parsed = apiErrorSchema.safeParse(error.response.data)
  if (parsed.success) return parsed.data.code

  return error.response.status === REQUEST_ERROR.TOO_MANY_REQUESTS_STATUS
    ? REQUEST_ERROR.TOO_MANY_REQUESTS
    : REQUEST_ERROR.UNKNOWN
}
