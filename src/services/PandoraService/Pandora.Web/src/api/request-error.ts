import { AxiosError } from 'axios'
import { REQUEST_ERROR } from '@/constants/api'
import { SERVER_ERROR_KEYS } from '@/constants/i18n'

export type RequestError = number | string

function getServerCode(error: AxiosError): string | undefined {
  const body = error.response?.data as { code?: unknown } | undefined
  const code = body?.code
  return typeof code === 'string' ? code : undefined
}

export function toRequestError(error: unknown): RequestError {
  if (!(error instanceof AxiosError)) return REQUEST_ERROR.UNKNOWN
  if (!error.response) return REQUEST_ERROR.NETWORK
  const serverCode = getServerCode(error)
  if (serverCode && serverCode in SERVER_ERROR_KEYS) return serverCode
  return error.response.status
}
