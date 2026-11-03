import { AxiosError } from 'axios'
import { REQUEST_ERROR } from '@/constants/api'

export type RequestError = number | (typeof REQUEST_ERROR)[keyof typeof REQUEST_ERROR]

export function toRequestError(error: unknown): RequestError {
  if (!(error instanceof AxiosError)) return REQUEST_ERROR.UNKNOWN
  if (!error.response) return REQUEST_ERROR.NETWORK
  return error.response.status
}
