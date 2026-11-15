import { createI18n } from '@shared/i18n/create-i18n'
import en from './locales/en.json'
import ru from './locales/ru.json'

export const i18n = createI18n({ resources: { ru, en } })
