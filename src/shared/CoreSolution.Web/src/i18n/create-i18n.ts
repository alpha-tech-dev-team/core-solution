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

export function createI18n(options: I18nOptions): I18n {
  const locale = ref<Locale>(options.defaultLocale ?? DEFAULT_LOCALE)

  function has(key: string): boolean {
    return options.resources[locale.value][key] !== undefined
  }

  function tr(key: string, ...params: readonly unknown[]): string {
    const template = options.resources[locale.value][key] ?? key

    return interpolate(template, params)
  }

  function trStatus(status: string): string {
    return tr(`${STATUS_KEY_PREFIX}_${status.toLowerCase()}`)
  }

  function trValidationMessage(message: ValidationMessage): string {
    return tr(`${VALIDATION_KEY_PREFIX}_${message.rule}`, message.limit)
  }

  function trError(error: unknown): string {
    if (!(error instanceof ApiError)) return tr(UNKNOWN_ERROR_KEY)
    if (error.status === null) return tr(NETWORK_ERROR_KEY)

    if (error.code !== null) {
      const codeKey = `${ERROR_CODE_KEY_PREFIX}_${error.code}`
      if (has(codeKey)) return tr(codeKey)
    }

    const statusKey = `${ERROR_STATUS_KEY_PREFIX}_${error.status}`

    return tr(has(statusKey) ? statusKey : UNKNOWN_ERROR_KEY)
  }

  const translations: Translations = {
    locale,
    setLocale: (nextLocale) => {
      locale.value = nextLocale
    },
    tr,
    trStatus,
    trValidationMessage,
    trError,
  }

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
