import { fileURLToPath, URL } from 'node:url'

import { defineConfig, type ProxyOptions } from 'vite'
import vue from '@vitejs/plugin-vue'

import config from './appsettings.json' with { type: 'json' }

function toPath(packageName: string): string {
  return fileURLToPath(new URL(`./node_modules/${packageName}`, import.meta.url))
}

const WEBSOCKET_PROXY_PREFIX = '/hubs'

function toProxyOptions(target: string, websocket: boolean): ProxyOptions {
  return {
    target,
    ...(target.startsWith('https') ? { changeOrigin: true, secure: false } : {}),
    ...(websocket ? { ws: true } : {}),
  }
}

const proxy = Object.fromEntries(
  Object.entries(config.proxy).map(([prefix, target]) => [
    prefix,
    toProxyOptions(target, prefix.startsWith(WEBSOCKET_PROXY_PREFIX)),
  ]),
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
