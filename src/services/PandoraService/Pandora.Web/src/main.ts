import '@shared/styles/index.css'

import App from '@/App.vue'
import { routes } from '@/router/routes'
import { i18n } from '@/i18n'
import { appSession } from '@/session'
import { mountWebApp } from '@shared/app/mount-web-app'
import { APP_MOUNT_POINT } from '@shared/constants/app'

mountWebApp({
  root: App,
  routes,
  i18n,
  appSession,
  mountPoint: APP_MOUNT_POINT,
})
