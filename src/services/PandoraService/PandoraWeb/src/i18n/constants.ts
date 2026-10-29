export const DEFAULT_LOCALE = 'ru'
export const SUPPORTED_LOCALES = ['ru', 'en'] as const

export type Locale = (typeof SUPPORTED_LOCALES)[number]

export const RESOURCE_XML_ROOT_TAG = 'res'
export const RESOURCE_XML_NAME_ATTR = 'name'
export const RESOURCE_XML_VALUE_TAG = 'value'
export const RESOURCE_XML_MIME = 'application/xml'
export const PLACEHOLDER_PATTERN = /\{(\d+)\}/g