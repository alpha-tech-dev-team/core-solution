import { DEFAULT_LOCALE, PLACEHOLDER_PATTERN, RESOURCE_XML_MIME, RESOURCE_XML_NAME_ATTR, RESOURCE_XML_ROOT_TAG, RESOURCE_XML_VALUE_TAG, type Locale } from './constants'
import { ERROR_KEY_PREFIX, VALIDATION_KEY_PREFIX, type ValidationRule } from './keys'
import ruResources from './resources/ru.resx?raw'
import enResources from './resources/en.resx?raw'

type ResourceMap = Record<string, string>

const RAW_RESOURCES: Record<Locale, string> = {
  ru: ruResources,
  en: enResources
}

function parseResx(xml: string): ResourceMap {
  const document = new DOMParser().parseFromString(xml, RESOURCE_XML_MIME)
  const resources: ResourceMap = {}

  for (const node of document.querySelectorAll(RESOURCE_XML_ROOT_TAG)) {
    const key = node.getAttribute(RESOURCE_XML_NAME_ATTR)
    const value = node.querySelector(RESOURCE_XML_VALUE_TAG)?.textContent
    if (key && value) {
      resources[key] = value
    }
  }

  return resources
}

function interpolate(template: string, params: readonly unknown[]): string {
  if (params.length === 0) return template
  return template.replace(PLACEHOLDER_PATTERN, (_, index) => String(params[Number(index)] ?? ''))
}

const resources: Record<Locale, ResourceMap> = {
  ru: parseResx(RAW_RESOURCES.ru),
  en: parseResx(RAW_RESOURCES.en)
}

let currentLocale: Locale = DEFAULT_LOCALE

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