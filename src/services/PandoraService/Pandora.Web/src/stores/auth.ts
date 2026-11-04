import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import api from '@/api/client'
import { AUTH_ENDPOINTS } from '@/constants/api'
import { AUTH_STORE_ID } from '@/constants/stores'
import { toRequestError, type RequestError } from '@/api/request-error'
import { clearSession, loadSession, saveSession } from './auth.session'
import type { AuthSession } from './models/auth-session'

function isExpired(expiresAt: string): boolean {
  return new Date(expiresAt) <= new Date()
}

export const useAuthStore = defineStore(AUTH_STORE_ID, () => {
  const accessTokenExpiresAt = ref<string | null>(null)

  const isAuthenticated = computed(() => {
    if (!accessTokenExpiresAt.value) return false
    return !isExpired(accessTokenExpiresAt.value)
  })

  function applySession(session: AuthSession): void {
    accessTokenExpiresAt.value = session.accessTokenExpiresAt
    saveSession(session)
  }

  async function login(username: string, password: string): Promise<RequestError | null> {
    try {
      const { data } = await api.post<AuthSession>(AUTH_ENDPOINTS.LOGIN, { username, password })
      applySession(data)
      return null
    } catch (error) {
      return toRequestError(error)
    }
  }

  async function refreshTokens(): Promise<boolean> {
    if (!accessTokenExpiresAt.value) return false
    try {
      const { data } = await api.post<AuthSession>(AUTH_ENDPOINTS.REFRESH)
      applySession(data)
      return true
    } catch {
      return false
    }
  }

  function restoreSession(): void {
    const session = loadSession()
    if (!session) return
    accessTokenExpiresAt.value = session.accessTokenExpiresAt
  }

  function clearLocalSession(): void {
    accessTokenExpiresAt.value = null
    clearSession()
  }

  async function logout(): Promise<void> {
    try {
      await api.post(AUTH_ENDPOINTS.LOGOUT)
    } catch {
    } finally {
      clearLocalSession()
    }
  }

  return {
    accessTokenExpiresAt,
    isAuthenticated,
    login,
    refreshTokens,
    restoreSession,
    clearLocalSession,
    logout
  }
})
