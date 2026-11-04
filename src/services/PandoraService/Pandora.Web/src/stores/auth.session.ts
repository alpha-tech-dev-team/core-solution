import { SESSION_STORAGE_KEY } from '@/constants/storage'
import type { AuthSession } from './models/auth-session'

export function saveSession(session: AuthSession): void {
  localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session))
}

export function loadSession(): AuthSession | null {
  const raw = localStorage.getItem(SESSION_STORAGE_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as AuthSession
  } catch {
    localStorage.removeItem(SESSION_STORAGE_KEY)
    return null
  }
}

export function clearSession(): void {
  localStorage.removeItem(SESSION_STORAGE_KEY)
}
