import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import api from '@/api/client'
import { AUTH_ENDPOINTS } from '@/api/constants'
import { clearSession, loadSession, saveSession } from './auth.session'
import { toLoginError, type LoginError } from './auth.errors'
import type { AuthTokens } from './auth.types'

const STORE_ID = 'auth'

function isExpired(expiresAt: string): boolean {
  return new Date(expiresAt) <= new Date()
}

export const useAuthStore = defineStore(STORE_ID, () => {
  const accessToken = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const accessTokenExpiresAt = ref<string | null>(null)

  const isAuthenticated = computed(() => {
    if (!accessToken.value || !accessTokenExpiresAt.value) return false
    return !isExpired(accessTokenExpiresAt.value)
  })

  function applyTokens(tokens: AuthTokens): void {
    accessToken.value = tokens.accessToken
    refreshToken.value = tokens.refreshToken
    accessTokenExpiresAt.value = tokens.accessTokenExpiresAt
    saveSession(tokens)
  }

  async function login(username: string, password: string): Promise<LoginError | null> {
    try {
      const { data } = await api.post<AuthTokens>(AUTH_ENDPOINTS.LOGIN, { username, password })
      applyTokens(data)
      return null
    } catch (error) {
      return toLoginError(error)
    }
  }

  async function refreshTokens(): Promise<boolean> {
    if (!refreshToken.value) return false
    try {
      const { data } = await api.post<AuthTokens>(AUTH_ENDPOINTS.REFRESH, {
        refreshToken: refreshToken.value
      })
      applyTokens(data)
      return true
    } catch {
      return false
    }
  }

  function restoreSession(): void {
    const session = loadSession()
    if (!session) return
    accessToken.value = session.accessToken
    refreshToken.value = session.refreshToken
    accessTokenExpiresAt.value = session.accessTokenExpiresAt
  }

  function logout(): void {
    accessToken.value = null
    refreshToken.value = null
    accessTokenExpiresAt.value = null
    clearSession()
  }

  return {
    accessToken,
    refreshToken,
    accessTokenExpiresAt,
    isAuthenticated,
    login,
    refreshTokens,
    restoreSession,
    logout
  }
})