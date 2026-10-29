import type { AuthTokens, StoredSession } from './auth.types'

const STORAGE_KEY = 'auth_tokens'

const TOKEN_FIELDS = {
  accessToken: 'accessToken',
  refreshToken: 'refreshToken',
  accessTokenExpiresAt: 'accessTokenExpiresAt'
} as const

export function saveSession(tokens: AuthTokens): void {
  const session: StoredSession = {
    [TOKEN_FIELDS.accessToken]: tokens.accessToken,
    [TOKEN_FIELDS.refreshToken]: tokens.refreshToken,
    [TOKEN_FIELDS.accessTokenExpiresAt]: tokens.accessTokenExpiresAt
  }
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function loadSession(): StoredSession | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as StoredSession
  } catch {
    localStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function clearSession(): void {
  localStorage.removeItem(STORAGE_KEY)
}