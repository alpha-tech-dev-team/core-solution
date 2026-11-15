import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { HTTP_STATUS } from '@shared/api/http'
import { isExpired, toEpochMs } from '@shared/auth/expiry'
import { readJson, removeJson, writeJson } from '@shared/storage/local-storage'
import { STORAGE_KEY, STORE_ID, TOKEN_REFRESH } from '@/constants/auth'
import { identityClient } from '@/api/identity-client'
import { ApiError } from '@/api/api-error'
import type { SessionResponse } from '@/api/contracts/session-response'

export const useAuthStore = defineStore(STORE_ID.AUTH, () => {
  const accessTokenExpiresAt = ref<number | null>(
    toEpochMs(readJson<string | number>(STORAGE_KEY.ACCESS_EXPIRES_AT)),
  )
  const nowMs = ref(Date.now())

  const isAuthenticated = computed(
    () => accessTokenExpiresAt.value !== null && !isAccessTokenExpired(),
  )

  let refreshTimer: number | undefined
  let clockTimer: number | undefined
  let refreshPromise: Promise<boolean> | null = null
  let sessionVersion = 0

  function isAccessTokenExpired(): boolean {
    return isExpired(accessTokenExpiresAt.value, nowMs.value)
  }

  function isUnauthorized(error: unknown): boolean {
    return error instanceof ApiError && error.status === HTTP_STATUS.UNAUTHORIZED
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

  function scheduleTokenRefresh(): void {
    clearRefreshTimer()

    const expiresAt = accessTokenExpiresAt.value
    if (expiresAt === null) return

    const delay = Math.max(
      expiresAt - Date.now() - TOKEN_REFRESH.MARGIN_MS,
      TOKEN_REFRESH.MIN_DELAY_MS,
    )

    refreshTimer = window.setTimeout(() => {
      void refreshSession()
    }, delay)
  }

  function persistExpiry(): void {
    if (accessTokenExpiresAt.value === null) {
      removeJson(STORAGE_KEY.ACCESS_EXPIRES_AT)
      return
    }

    writeJson(STORAGE_KEY.ACCESS_EXPIRES_AT, accessTokenExpiresAt.value)
  }

  async function performRefresh(): Promise<boolean> {
    if (accessTokenExpiresAt.value === null) return false

    const version = sessionVersion

    try {
      const session = await identityClient.refresh()
      if (sessionVersion !== version) return false

      setSession(session)
      return true
    } catch (error) {
      if (isUnauthorized(error)) clearSessionState()

      return false
    }
  }

  function refreshSession(): Promise<boolean> {
    if (!refreshPromise) {
      refreshPromise = performRefresh().finally(() => {
        refreshPromise = null
      })
    }

    return refreshPromise
  }

  function setSession(session: SessionResponse): void {
    sessionVersion += 1
    accessTokenExpiresAt.value = toEpochMs(session.accessTokenExpiresAt)
    persistExpiry()

    startClock()
    scheduleTokenRefresh()
  }

  function clearSessionState(): void {
    sessionVersion += 1
    accessTokenExpiresAt.value = null
    persistExpiry()
  }

  async function logout(): Promise<void> {
    clearRefreshTimer()
    stopClock()

    await identityClient.logout().catch(() => undefined)
    clearSessionState()
  }

  async function restoreSession(): Promise<void> {
    if (accessTokenExpiresAt.value === null) {
      clearSessionState()
      return
    }

    startClock()

    if (isAccessTokenExpired()) {
      await refreshSession()
      return
    }

    scheduleTokenRefresh()
  }

  return {
    accessTokenExpiresAt,
    isAuthenticated,
    setSession,
    logout,
    refreshSession,
    restoreSession,
  }
})
