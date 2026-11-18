import { fileURLToPath, URL } from 'node:url'

import { defineConfig, type ProxyOptions } from 'vite'
import vue from '@vitejs/plugin-vue'

import config from './appsettings.json' with { type: 'json' }

function toPath(packageName: string): string {
  return fileURLToPath(new URL(`./node_modules/${packageName}`, import.meta.url))
}

function toProxyOptions(target: string): ProxyOptions {
  return target.startsWith('https') ? { target, changeOrigin: true, secure: false } : { target }
}

const proxy = Object.fromEntries(
  Object.entries(config.proxy).map(([prefix, target]) => [prefix, toProxyOptions(target)]),
)

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@shared': fileURLToPath(
        new URL('../../../../src/shared/CoreSolution.Web/src', import.meta.url),
      ),
      vue: toPath('vue'),
      pinia: toPath('pinia'),
      'vue-router': toPath('vue-router'),
      zod: toPath('zod'),
    },
  },
  server: {
    proxy,
  },
  preview: {
    proxy,
  },
})
