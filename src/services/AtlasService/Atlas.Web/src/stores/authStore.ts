import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { StorageKey, StoreId, TokenRefresh } from '@/constants/auth.Constants'
import { HttpStatus } from '@/constants/http.Constants'
import { identityClient } from '@/api/identityClient'
import { ApiError } from '@/api/ApiError'
import type { AuthTokensResponse } from '@/api/contracts/AuthTokensResponse'
import { getTokenExpiryMs, isExpired } from '@/utils/jwt'

function parseExpiry(value: string | null | undefined): number | null {
  if (!value) return null

  const numeric = Number(value)
  if (!Number.isNaN(numeric)) return numeric

  const parsed = Date.parse(value)
  return Number.isNaN(parsed) ? null : parsed
}

export const useAuthStore = defineStore(StoreId.Auth, () => {
  const token = ref<string | null>(localStorage.getItem(StorageKey.AuthToken))
  const refreshToken = ref<string | null>(localStorage.getItem(StorageKey.RefreshToken))
  const accessTokenExpiresAt = ref<number | null>(
    parseExpiry(localStorage.getItem(StorageKey.AccessExpiresAt)),
  )
  const nowMs = ref(Date.now())

  const isAuthenticated = computed(() => !!token.value && !isAccessTokenExpired())

  let refreshTimer: number | undefined
  let clockTimer: number | undefined
  let refreshPromise: Promise<boolean> | null = null

  function resolveExpiresAt(): number | null {
    return accessTokenExpiresAt.value ?? (token.value ? getTokenExpiryMs(token.value) : null)
  }

  function isAccessTokenExpired(): boolean {
    return !token.value || isExpired(resolveExpiresAt(), nowMs.value)
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

    const expiresAt = resolveExpiresAt()
    if (expiresAt === null || !refreshToken.value) return

    const delay = Math.max(expiresAt - Date.now() - TokenRefresh.MarginMs, TokenRefresh.MinDelayMs)

    refreshTimer = window.setTimeout(() => {
      void refreshSession()
    }, delay)
  }

  async function performRefresh(): Promise<boolean> {
    const currentRefreshToken = refreshToken.value

    if (!currentRefreshToken) {
      logout()
      return false
    }

    try {
      const session = await identityClient.refresh(currentRefreshToken)

      if (refreshToken.value !== currentRefreshToken) {
        return false
      }

      setSession(session)
      return true
    } catch (error) {
      if (refreshToken.value === currentRefreshToken && isUnauthorized(error)) {
        logout()
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

  function setSession(session: AuthTokensResponse) {
    token.value = session.accessToken
    refreshToken.value = session.refreshToken
    accessTokenExpiresAt.value =
      parseExpiry(session.accessTokenExpiresAt) ?? getTokenExpiryMs(session.accessToken)

    localStorage.setItem(StorageKey.AuthToken, session.accessToken)
    localStorage.setItem(StorageKey.RefreshToken, session.refreshToken)

    if (accessTokenExpiresAt.value === null) {
      localStorage.removeItem(StorageKey.AccessExpiresAt)
    } else {
      localStorage.setItem(StorageKey.AccessExpiresAt, String(accessTokenExpiresAt.value))
    }

    startClock()
    scheduleTokenRefresh()
  }

  function logout() {
    clearRefreshTimer()
    stopClock()
    token.value = null
    refreshToken.value = null
    accessTokenExpiresAt.value = null
    localStorage.removeItem(StorageKey.AuthToken)
    localStorage.removeItem(StorageKey.RefreshToken)
    localStorage.removeItem(StorageKey.AccessExpiresAt)
  }

  async function restoreSession() {
    if (!token.value || !refreshToken.value) {
      logout()
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
    token,
    refreshToken,
    accessTokenExpiresAt,
    isAuthenticated,
    setSession,
    logout,
    refreshSession,
    restoreSession,
  }
})
