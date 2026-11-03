import {
  DEFAULT_LOCALE,
  ERROR_KEY_PREFIX,
  PLACEHOLDER_PATTERN,
  VALIDATION_KEY_PREFIX,
  type Locale,
  type ValidationRule
} from '@/constants/i18n'
import ru from './locales/ru.json'
import en from './locales/en.json'

type ResourceMap = Record<string, string>

const resources: Record<Locale, ResourceMap> = { ru, en }

let currentLocale: Locale = DEFAULT_LOCALE

function interpolate(template: string, params: readonly unknown[]): string {
  if (params.length === 0) return template
  return template.replace(PLACEHOLDER_PATTERN, (_, index) => String(params[Number(index)] ?? ''))
}

export function setLocale(locale: Locale): void {
  currentLocale = locale
}

export function getLocale(): Locale {
  return currentLocale
}

export function tr(key: string, ...params: readonly unknown[]): string {
  const template = resources[currentLocale][key] ?? key
  return interpolate(template, params)
}

export function trRequestError(error: number | string): string {
  return tr(`${ERROR_KEY_PREFIX}_${error}`)
}

export function trValidation(rule: ValidationRule, ...params: readonly unknown[]): string {
  return tr(`${VALIDATION_KEY_PREFIX}_${rule}`, ...params)
}
