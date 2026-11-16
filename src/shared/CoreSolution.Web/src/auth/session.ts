import { computed, inject, ref, type App, type InjectionKey } from 'vue'
import { defineStore } from 'pinia'
import { HTTP_STATUS } from '../api/http'
import { ApiError } from '../api/api-error'
import { AUTH_STORE_ID, AUTH_STORAGE_KEY, TOKEN_REFRESH } from '../constants/auth'
import type { ApiSession } from '../models/api-session'
import type { AuthSession } from '../models/auth-session'
import type { Credentials } from '../models/credentials'
import { readJson, removeJson, writeJson } from '../storage/local-storage'
import { createRequireAuthGuard, type RouteRedirect } from './auth-guard'
import { isExpired, toEpochMs } from './expiry'

export interface SessionGateway {
  login(credentials: Credentials): Promise<ApiSession>
  refresh(): Promise<ApiSession>
  logout(): Promise<void>
}

export interface SessionOptions {
  gateway: SessionGateway
  loginRouteName: string
  authenticatedRouteName: string
}

export interface SessionContext {
  useAuthStore: ReturnType<typeof defineAuthStore>
  loginRouteName: string
  authenticatedRouteName: string
}

export interface Session extends SessionContext {
  install(app: App): void
  requireAuth: () => Promise<true | RouteRedirect>
}

const SESSION_KEY: InjectionKey<SessionContext> = Symbol('session')

function defineAuthStore(options: SessionOptions) {
  return defineStore(AUTH_STORE_ID, () => {
    const session = ref<AuthSession | null>(readJson<AuthSession>(AUTH_STORAGE_KEY))
    const nowMs = ref(Date.now())

    const username = computed<string | null>(() => session.value?.username ?? null)
    const isAuthenticated = computed<boolean>(
      () => session.value !== null && !isExpired(expiresAtMs(), nowMs.value),
    )

    let refreshTimer: number | undefined
    let clockTimer: number | undefined
    let refreshPromise: Promise<boolean> | null = null
    let sessionVersion = 0

    function expiresAtMs(): number | null {
      return session.value === null ? null : toEpochMs(session.value.accessTokenExpiresAt)
    }

    function startClock(): void {
      if (clockTimer !== undefined) return

      clockTimer = window.setInterval(() => {
        nowMs.value = Date.now()
      }, TOKEN_REFRESH.CLOCK_TICK_MS)
    }

    function stopClock(): void {
      if (clockTimer === undefined) return

      window.clearInterval(clockTimer)
      clockTimer = undefined
    }

    function clearRefreshTimer(): void {
      if (refreshTimer === undefined) return

      window.clearTimeout(refreshTimer)
      refreshTimer = undefined
    }

    function scheduleRefresh(): void {
      clearRefreshTimer()

      const expiresAt = expiresAtMs()
      if (expiresAt === null) return

      const delay = Math.max(
        expiresAt - Date.now() - TOKEN_REFRESH.MARGIN_MS,
        TOKEN_REFRESH.MIN_DELAY_MS,
      )

      refreshTimer = window.setTimeout(() => {
        void refreshSession()
      }, delay)
    }

    function persist(): void {
      if (session.value === null) {
        removeJson(AUTH_STORAGE_KEY)
        return
      }

      writeJson(AUTH_STORAGE_KEY, session.value)
    }

    function applySession(apiSession: ApiSession, sessionUsername: string): void {
      sessionVersion += 1
      session.value = {
        accessTokenExpiresAt: apiSession.accessTokenExpiresAt,
        username: sessionUsername,
      }

      persist()
      startClock()
      scheduleRefresh()
    }

    function clearSession(): void {
      sessionVersion += 1
      session.value = null

      persist()
      clearRefreshTimer()
      stopClock()
    }

    async function performRefresh(): Promise<boolean> {
      const current = session.value
      if (current === null) return false

      const version = sessionVersion

      try {
        const refreshed = await options.gateway.refresh()
        if (sessionVersion !== version) return false

        applySession(refreshed, current.username)

        return true
      } catch (error) {
        if (error instanceof ApiError && error.status === HTTP_STATUS.UNAUTHORIZED) clearSession()

        return false
      }
    }

    function refreshSession(): Promise<boolean> {
      if (refreshPromise === null) {
        refreshPromise = performRefresh().finally(() => {
          refreshPromise = null
        })
      }

      return refreshPromise
    }

    async function login(credentials: Credentials): Promise<void> {
      const apiSession = await options.gateway.login(credentials)

      applySession(apiSession, credentials.username)
    }

    async function logout(): Promise<void> {
      await options.gateway.logout().catch(() => undefined)
      clearSession()
    }

    async function restoreSession(): Promise<void> {
      if (session.value === null) {
        clearSession()
        return
      }

      startClock()

      if (isAuthenticated.value) {
        scheduleRefresh()
        return
      }

      await refreshSession()
    }

    return {
      session,
      username,
      isAuthenticated,
      login,
      logout,
      refreshSession,
      restoreSession,
    }
  })
}

export function createSession(options: SessionOptions): Session {
  const useAuthStore = defineAuthStore(options)
  const context: SessionContext = {
    useAuthStore,
    loginRouteName: options.loginRouteName,
    authenticatedRouteName: options.authenticatedRouteName,
  }

  return {
    ...context,
    requireAuth: createRequireAuthGuard({
      isAuthenticated: () => useAuthStore().isAuthenticated,
      refreshSession: () => useAuthStore().refreshSession(),
      loginRouteName: options.loginRouteName,
    }),
    install(app) {
      app.provide(SESSION_KEY, context)
    },
  }
}

export function useSession(): SessionContext {
  const context = inject(SESSION_KEY)
  if (context === undefined) throw new Error('Session plugin is not installed')

  return context
}
