import { createI18n, mergeResources } from '@shared/i18n/create-i18n'
import sharedEn from '@shared/i18n/locales/en.json'
import sharedRu from '@shared/i18n/locales/ru.json'
import en from './locales/en.json'
import ru from './locales/ru.json'

export const i18n = createI18n({
  resources: mergeResources({ ru: sharedRu, en: sharedEn }, { ru, en }),
})
