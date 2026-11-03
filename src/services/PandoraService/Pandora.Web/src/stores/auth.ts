import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import api from '@/api/client'
import { AUTH_ENDPOINTS } from '@/constants/api'
import { AUTH_STORE_ID } from '@/constants/stores'
import { toRequestError, type RequestError } from '@/api/request-error'
import { clearSession, loadSession, saveSession } from './auth.session'
import type { AuthTokens } from './models/auth-tokens'

function isExpired(expiresAt: string): boolean {
  return new Date(expiresAt) <= new Date()
}

export const useAuthStore = defineStore(AUTH_STORE_ID, () => {
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

  async function login(username: string, password: string): Promise<RequestError | null> {
    try {
      const { data } = await api.post<AuthTokens>(AUTH_ENDPOINTS.LOGIN, { username, password })
      applyTokens(data)
      return null
    } catch (error) {
      return toRequestError(error)
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
