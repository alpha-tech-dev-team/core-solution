import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

const DEV_SERVER_PORT = 5173
const IDENTITY_SERVICE_URL = 'http://localhost:5257'
const API_PREFIX = '/api'

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
      [API_PREFIX]: IDENTITY_SERVICE_URL
    }
  }
})