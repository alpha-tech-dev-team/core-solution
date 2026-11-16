import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

const DEV_SERVER_PORT = 5173
const IDENTITY_SERVICE_URL = 'http://localhost:5257'
const PANDORA_API_SERVICE_URL = 'http://localhost:5008'
const API_PREFIX = '/api'
const AUDIT_PREFIX = '/api/admin/audit'
const SECRETS_PREFIX = '/api/admin/secrets'
const AUDIT_HUB_PREFIX = '/hubs/audit'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': '/src'
    }
  },
  server: {
    port: DEV_SERVER_PORT,
    proxy: {
      [AUDIT_PREFIX]: PANDORA_API_SERVICE_URL,
      [SECRETS_PREFIX]: PANDORA_API_SERVICE_URL,
      [AUDIT_HUB_PREFIX]: {
        target: PANDORA_API_SERVICE_URL,
        ws: true
      },
      [API_PREFIX]: IDENTITY_SERVICE_URL
    }
  }
})
