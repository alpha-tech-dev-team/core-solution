import { api } from '@shared/app/app-session'
import { CREDENTIAL_ACCESS_SETTINGS_ENDPOINTS } from '@/constants/api'
import type { CredentialAccessSettings } from '@/models/credential-access-settings'

export async function fetchCredentialAccessSettings(): Promise<CredentialAccessSettings> {
  return await api.get<CredentialAccessSettings>(CREDENTIAL_ACCESS_SETTINGS_ENDPOINTS.SETTINGS)
}

export async function updateCredentialAccessSettings(
  ttlMinutes: number,
): Promise<CredentialAccessSettings> {
  return await api.put<CredentialAccessSettings>(CREDENTIAL_ACCESS_SETTINGS_ENDPOINTS.SETTINGS, {
    ttlMinutes,
  })
}
