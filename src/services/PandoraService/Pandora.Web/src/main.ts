import { bootstrapApp } from '@shared/app/bootstrap-app'
import { routes } from '@/router/routes'
import ru from '@/i18n/locales/ru.json'
import en from '@/i18n/locales/en.json'

bootstrapApp({ routes, locales: { ru, en } })
