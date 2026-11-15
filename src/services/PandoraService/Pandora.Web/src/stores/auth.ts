import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { isExpired, toEpochMs } from '@shared/auth/expiry'
import { readJson, removeJson, writeJson } from '@shared/storage/local-storage'
import api from '@/api/client'
import { AUTH_ENDPOINTS } from '@/constants/api'
import { AUTH_STORE_ID } from '@/constants/stores'
import { SESSION_STORAGE_KEY } from '@/constants/storage'
import { toRequestError, type RequestError } from '@/api/request-error'
import type { AuthSession } from '@/models/auth-session'

type SessionPayload = Omit<AuthSession, 'username'>

export const useAuthStore = defineStore(AUTH_STORE_ID, () => {
  const accessTokenExpiresAt = ref<string | null>(null)
  const username = ref<string | null>(null)

  const isAuthenticated = computed(() => {
    if (!accessTokenExpiresAt.value) return false

    return !isExpired(toEpochMs(accessTokenExpiresAt.value), Date.now())
  })

  function applySession(session: AuthSession): void {
    accessTokenExpiresAt.value = session.accessTokenExpiresAt
    username.value = session.username
    writeJson(SESSION_STORAGE_KEY, session)
  }

  async function login(username: string, password: string): Promise<RequestError | null> {
    try {
      const { data } = await api.post<SessionPayload>(AUTH_ENDPOINTS.LOGIN, { username, password })
      applySession({ ...data, username })
      return null
    } catch (error) {
      return toRequestError(error)
    }
  }

  async function refreshTokens(): Promise<boolean> {
    if (!accessTokenExpiresAt.value) return false

    try {
      const { data } = await api.post<SessionPayload>(AUTH_ENDPOINTS.REFRESH)
      applySession({ ...data, username: username.value ?? '' })
      return true
    } catch {
      return false
    }
  }

  function restoreSession(): void {
    const session = readJson<AuthSession>(SESSION_STORAGE_KEY)
    if (session === null) return

    accessTokenExpiresAt.value = session.accessTokenExpiresAt
    username.value = session.username
  }

  function clearLocalSession(): void {
    accessTokenExpiresAt.value = null
    username.value = null
    removeJson(SESSION_STORAGE_KEY)
  }

  async function logout(): Promise<void> {
    await api.post(AUTH_ENDPOINTS.LOGOUT).catch(() => undefined)
    clearLocalSession()
  }

  return {
    accessTokenExpiresAt,
    username,
    isAuthenticated,
    login,
    refreshTokens,
    restoreSession,
    clearLocalSession,
    logout,
  }
})
