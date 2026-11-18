import { AUTH_STORAGE_KEY } from '../constants/auth'
import type { AuthSession } from '../models/auth-session'
import { readJson, removeJson, writeJson } from '../storage/local-storage'

export function readStoredSession(): AuthSession | null {
  return readJson<AuthSession>(AUTH_STORAGE_KEY)
}

export function persistSession(session: AuthSession | null): void {
  if (session === null) {
    removeJson(AUTH_STORAGE_KEY)
    return
  }

  writeJson(AUTH_STORAGE_KEY, session)
}
