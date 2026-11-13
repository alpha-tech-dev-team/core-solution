import {
  DEFAULT_LOCALE,
  ERROR_KEY_PREFIX,
  PLACEHOLDER_PATTERN,
  SERVER_ERROR_KEYS,
  VALIDATION_KEY_PREFIX,
  type Locale,
  type ValidationRule
} from '@/constants/i18n'
import { REQUEST_ERROR } from '@/constants/api'
import ru from './locales/ru.json'
import en from './locales/en.json'

type ResourceMap = Record<string, string>

const resources: Record<Locale, ResourceMap> = { ru, en }

export type ResourceKey = keyof typeof ru

let currentLocale: Locale = DEFAULT_LOCALE

function interpolate(template: string, params: readonly unknown[]): string {
  if (params.length === 0) return template
  return template.replace(PLACEHOLDER_PATTERN, (_, index) => String(params[Number(index)] ?? ''))
}

function lookup(key: string, params: readonly unknown[] = []): string {
  const template = resources[currentLocale][key] ?? key
  return interpolate(template, params)
}

export function setLocale(locale: Locale): void {
  currentLocale = locale
}

export function getLocale(): Locale {
  return currentLocale
}

export function tr(key: ResourceKey, ...params: readonly unknown[]): string {
  return lookup(key, params)
}

export function trRequestError(error: string): string {
  const serverKey = SERVER_ERROR_KEYS[error]
  if (serverKey) return lookup(serverKey)

  const errorKey = `${ERROR_KEY_PREFIX}_${error}`
  if (resources[currentLocale][errorKey] !== undefined) return lookup(errorKey)

  return lookup(`${ERROR_KEY_PREFIX}_${REQUEST_ERROR.UNKNOWN}`)
}

export function trStatus(status: string): string {
  return lookup(`status_${status.toLowerCase()}`)
}

export function trValidation(rule: ValidationRule, ...params: readonly unknown[]): string {
  return lookup(`${VALIDATION_KEY_PREFIX}_${rule}`, params)
}
