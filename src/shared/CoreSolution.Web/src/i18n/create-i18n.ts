import { inject, ref, type App, type InjectionKey, type Ref } from 'vue'
import { ApiError } from '../api/api-error'
import {
  ERROR_CODE_KEY_PREFIX,
  ERROR_STATUS_KEY_PREFIX,
  NETWORK_ERROR_KEY,
  PLACEHOLDER_PATTERN,
  STATUS_KEY_PREFIX,
  UNKNOWN_ERROR_KEY,
  VALIDATION_KEY_PREFIX,
} from '../constants/i18n'
import { DEFAULT_LOCALE, type Locale } from './locale'
import type { ValidationMessage } from '../validation/validation-message'

export type Resources = Record<Locale, Record<string, string>>

export interface I18nOptions {
  resources: Resources
  defaultLocale?: Locale
}

export function mergeResources(
  base: Resources,
  overrides: Partial<Record<Locale, Record<string, string>>>,
): Resources {
  const locales = Object.keys(base) as Locale[]

  return Object.fromEntries(
    locales.map((locale) => [locale, { ...base[locale], ...(overrides[locale] ?? {}) }]),
  ) as Resources
}

export interface Translations {
  locale: Ref<Locale>
  setLocale(locale: Locale): void
  tr(key: string, ...params: readonly unknown[]): string
  trStatus(status: string): string
  trValidationMessage(message: ValidationMessage): string
  trError(error: unknown): string
}

export interface I18n extends Translations {
  install(app: App): void
}

const TRANSLATIONS_KEY: InjectionKey<Translations> = Symbol('translations')

function interpolate(template: string, params: readonly unknown[]): string {
  if (params.length === 0) return template

  return template.replace(PLACEHOLDER_PATTERN, (_, index) => String(params[Number(index)] ?? ''))
}

function resolveErrorKey(error: ApiError, has: (key: string) => boolean): string {
  if (error.status === null) return NETWORK_ERROR_KEY

  if (error.code !== null) {
    const codeKey = `${ERROR_CODE_KEY_PREFIX}_${error.code}`
    if (has(codeKey)) return codeKey
  }

  const statusKey = `${ERROR_STATUS_KEY_PREFIX}_${error.status}`

  return has(statusKey) ? statusKey : UNKNOWN_ERROR_KEY
}

function createTranslations(options: I18nOptions): Translations {
  const locale = ref<Locale>(options.defaultLocale ?? DEFAULT_LOCALE)

  function has(key: string): boolean {
    return options.resources[locale.value][key] !== undefined
  }

  function tr(key: string, ...params: readonly unknown[]): string {
    return interpolate(options.resources[locale.value][key] ?? key, params)
  }

  return {
    locale,
    setLocale: (nextLocale) => {
      locale.value = nextLocale
    },
    tr,
    trStatus: (status) => tr(`${STATUS_KEY_PREFIX}_${status.toLowerCase()}`),
    trValidationMessage: (message) => tr(`${VALIDATION_KEY_PREFIX}_${message.rule}`, message.limit),
    trError: (error) =>
      tr(error instanceof ApiError ? resolveErrorKey(error, has) : UNKNOWN_ERROR_KEY),
  }
}

export function createI18n(options: I18nOptions): I18n {
  const translations = createTranslations(options)

  return {
    ...translations,
    install(app) {
      app.provide(TRANSLATIONS_KEY, translations)
    },
  }
}

export function useTranslations(): Translations {
  const translations = inject(TRANSLATIONS_KEY)
  if (translations === undefined) throw new Error('I18n plugin is not installed')

  return translations
}
