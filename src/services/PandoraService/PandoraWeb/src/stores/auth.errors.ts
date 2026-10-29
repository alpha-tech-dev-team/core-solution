import { AxiosError } from 'axios'
import { HTTP_STATUS } from '@/api/http-status'

export const LOGIN_ERROR = {
  INVALID_CREDENTIALS: HTTP_STATUS.UNAUTHORIZED,
  NETWORK: 'network',
  UNKNOWN: 'unknown'
} as const

export type LoginError = number | typeof LOGIN_ERROR.NETWORK | typeof LOGIN_ERROR.UNKNOWN

export function toLoginError(error: unknown): LoginError {
  if (!(error instanceof AxiosError)) return LOGIN_ERROR.UNKNOWN
  if (!error.response) return LOGIN_ERROR.NETWORK
  return error.response.status
}