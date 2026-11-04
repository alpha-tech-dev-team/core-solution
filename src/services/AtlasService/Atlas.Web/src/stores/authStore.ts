import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { StorageKey, StoreId, TokenRefresh } from '@/constants/auth.Constants'
import { HttpStatus } from '@/constants/http.Constants'
import { identityClient } from '@/api/identityClient'
import { ApiError } from '@/api/ApiError'
import type { SessionResponse } from '@/api/contracts/SessionResponse'
import { isExpired } from '@/utils/jwt'

function parseExpiry(value: string | null | undefined): number | null {
  if (!value) return null

  const numeric = Number(value)
  if (!Number.isNaN(numeric)) return numeric

  const parsed = Date.parse(value)
  return Number.isNaN(parsed) ? null : parsed
}

export const useAuthStore = defineStore(StoreId.Auth, () => {
  const accessTokenExpiresAt = ref<number | null>(
    parseExpiry(localStorage.getItem(StorageKey.AccessExpiresAt)),
  )
  const nowMs = ref(Date.now())

  const isAuthenticated = computed(
    () => accessTokenExpiresAt.value !== null && !isAccessTokenExpired(),
  )

  let refreshTimer: number | undefined
  let clockTimer: number | undefined
  let refreshPromise: Promise<boolean> | null = null

  function isAccessTokenExpired(): boolean {
    return isExpired(accessTokenExpiresAt.value, nowMs.value)
  }

  function isUnauthorized(error: unknown): boolean {
    return error instanceof ApiError && error.status === HttpStatus.Unauthorized
  }

  function startClock() {
    if (clockTimer !== undefined) return

    clockTimer = window.setInterval(() => {
      nowMs.value = Date.now()
    }, TokenRefresh.ClockTickMs)
  }

  function stopClock() {
    if (clockTimer === undefined) return

    window.clearInterval(clockTimer)
    clockTimer = undefined
  }

  function clearRefreshTimer() {
    if (refreshTimer !== undefined) {
      window.clearTimeout(refreshTimer)
      refreshTimer = undefined
    }
  }

  function scheduleTokenRefresh() {
    clearRefreshTimer()

    const expiresAt = accessTokenExpiresAt.value
    if (expiresAt === null) return

    const delay = Math.max(expiresAt - Date.now() - TokenRefresh.MarginMs, TokenRefresh.MinDelayMs)

    refreshTimer = window.setTimeout(() => {
      void refreshSession()
    }, delay)
  }

  let sessionVersion = 0

  async function performRefresh(): Promise<boolean> {
    if (accessTokenExpiresAt.value === null) return false

    const version = sessionVersion

    try {
      const session = await identityClient.refresh()
      if (sessionVersion !== version) return false
      setSession(session)
      return true
    } catch (error) {
      if (isUnauthorized(error)) {
        clearSessionState()
      }
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

  function setSession(session: SessionResponse) {
    sessionVersion += 1
    accessTokenExpiresAt.value = parseExpiry(session.accessTokenExpiresAt)

    if (accessTokenExpiresAt.value === null) {
      localStorage.removeItem(StorageKey.AccessExpiresAt)
    } else {
      localStorage.setItem(StorageKey.AccessExpiresAt, String(accessTokenExpiresAt.value))
    }

    startClock()
    scheduleTokenRefresh()
  }

  function clearSessionState() {
    sessionVersion += 1
    accessTokenExpiresAt.value = null
    localStorage.removeItem(StorageKey.AccessExpiresAt)
  }

  async function logout() {
    clearRefreshTimer()
    stopClock()
    try {
      await identityClient.logout()
    } catch {
      clearSessionState()
      return
    }
    clearSessionState()
  }

  async function restoreSession() {
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
