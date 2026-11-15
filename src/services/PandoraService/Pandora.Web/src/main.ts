import '@shared/styles/index.css'

import App from '@/App.vue'
import router from '@/router'
import { i18n } from '@/i18n'
import { session } from '@/session'
import { mountWebApp } from '@shared/app/mount-web-app'
import { APP_MOUNT_POINT } from '@shared/constants/app'

mountWebApp({
  root: App,
  router,
  i18n,
  session,
  mountPoint: APP_MOUNT_POINT,
})
