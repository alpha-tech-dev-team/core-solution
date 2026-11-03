import { SESSION_FIELDS, SESSION_STORAGE_KEY } from '@/constants/storage'
import type { AuthTokens } from './models/auth-tokens'
import type { StoredSession } from './models/stored-session'

export function saveSession(tokens: AuthTokens): void {
  const session: StoredSession = {
    [SESSION_FIELDS.accessToken]: tokens.accessToken,
    [SESSION_FIELDS.refreshToken]: tokens.refreshToken,
    [SESSION_FIELDS.accessTokenExpiresAt]: tokens.accessTokenExpiresAt
  }
  localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session))
}

export function loadSession(): StoredSession | null {
  const raw = localStorage.getItem(SESSION_STORAGE_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as StoredSession
  } catch {
    localStorage.removeItem(SESSION_STORAGE_KEY)
    return null
  }
}

export function clearSession(): void {
  localStorage.removeItem(SESSION_STORAGE_KEY)
}
