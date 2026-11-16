import { computed, inject, ref, type App, type ComputedRef, type InjectionKey, type Ref } from 'vue'
import { defineStore } from 'pinia'
import { AUTH_STORE_ID } from '../constants/auth'
import type { ApiSession } from '../models/api-session'
import type { AuthSession } from '../models/auth-session'
import type { Credentials } from '../models/credentials'
import { createRequireAuthGuard, type RouteRedirect } from './auth-guard'
import { isExpired, toEpochMs } from './expiry'
import { createRefreshCoordinator } from './session-refresh'
import { createSessionTimers } from './session-timers'
import { persistSession, readStoredSession } from './session-storage'

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

export interface SessionState {
  session: Ref<AuthSession | null>
  username: ComputedRef<string | null>
  isAuthenticated: ComputedRef<boolean>
  login(credentials: Credentials): Promise<void>
  logout(): Promise<void>
  refreshSession(): Promise<boolean>
  restoreSession(): Promise<void>
}

const SESSION_KEY: InjectionKey<SessionContext> = Symbol('session')

interface SessionLifetimeOptions {
  session: Ref<AuthSession | null>
  timers: ReturnType<typeof createSessionTimers>
  scheduleRefresh: () => void
  bumpVersion: () => void
}

function createSessionLifetime(options: SessionLifetimeOptions) {
  function apply(apiSession: ApiSession, sessionUsername: string): void {
    options.bumpVersion()
    options.session.value = {
      accessTokenExpiresAt: apiSession.accessTokenExpiresAt,
      username: sessionUsername,
    }

    persistSession(options.session.value)
    options.timers.start()
    options.scheduleRefresh()
  }

  function clear(): void {
    options.bumpVersion()
    options.session.value = null

    persistSession(null)
    options.timers.cancelAll()
  }

  return { apply, clear }
}

export function createSessionState(gateway: SessionGateway): SessionState {
  const session = ref<AuthSession | null>(readStoredSession())
  const nowMs = ref(Date.now())
  const timers = createSessionTimers(nowMs)

  let sessionVersion = 0

  const username = computed<string | null>(() => session.value?.username ?? null)
  const isAuthenticated = computed<boolean>(
    () => session.value !== null && !isExpired(expiresAtMs(), nowMs.value),
  )

  function expiresAtMs(): number | null {
    return session.value === null ? null : toEpochMs(session.value.accessTokenExpiresAt)
  }

  function scheduleRefresh(): void {
    const expiresAt = expiresAtMs()
    if (expiresAt !== null) timers.scheduleRefresh(() => void refreshSession(), expiresAt)
  }

  const lifetime = createSessionLifetime({
    session,
    timers,
    scheduleRefresh,
    bumpVersion: () => {
      sessionVersion += 1
    },
  })

  const refreshCoordinator = createRefreshCoordinator({
    refresh: () => gateway.refresh(),
    currentVersion: () => sessionVersion,
    applyRefreshed: (refreshed) => {
      if (session.value !== null) lifetime.apply(refreshed, session.value.username)
    },
    clear: lifetime.clear,
  })

  function refreshSession(): Promise<boolean> {
    return session.value === null ? Promise.resolve(false) : refreshCoordinator.refresh()
  }

  async function login(credentials: Credentials): Promise<void> {
    lifetime.apply(await gateway.login(credentials), credentials.username)
  }

  async function logout(): Promise<void> {
    await gateway.logout().catch(() => undefined)
    lifetime.clear()
  }

  async function restoreSession(): Promise<void> {
    if (session.value === null) {
      lifetime.clear()
      return
    }

    timers.start()

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
}

function defineAuthStore(gateway: SessionGateway) {
  return defineStore(AUTH_STORE_ID, (): SessionState => createSessionState(gateway))
}

export function createSession(options: SessionOptions): Session {
  const useAuthStore = defineAuthStore(options.gateway)
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
