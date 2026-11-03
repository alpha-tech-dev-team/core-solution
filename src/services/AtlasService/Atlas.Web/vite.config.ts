import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

import config from './appsettings.json' with { type: 'json' }

const identityProxy = {
  [config.proxy.prefix]: {
    target: config.proxy.target,
    changeOrigin: true,
    secure: false,
  },
}

export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    proxy: identityProxy,
  },
  preview: {
    proxy: identityProxy,
  },
})
