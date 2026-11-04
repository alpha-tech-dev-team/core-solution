import api from './client'
import { SECRETS_ENDPOINTS } from '@/constants/api'
import type { SecretItem } from '@/stores/models/secret-item'

export interface CreateSecretPayload {
  name: string
  username: string
  password: string
}

export interface UpdateSecretPayload {
  name: string
  username: string
  password: string | null
}

export async function fetchSecrets(): Promise<SecretItem[]> {
  const { data } = await api.get<SecretItem[]>(SECRETS_ENDPOINTS.COLLECTION)
  return data
}

export async function createSecret(payload: CreateSecretPayload): Promise<SecretItem> {
  const { data } = await api.post<SecretItem>(SECRETS_ENDPOINTS.COLLECTION, payload)
  return data
}

export async function updateSecret(itemId: string, payload: UpdateSecretPayload): Promise<SecretItem> {
  const { data } = await api.put<SecretItem>(`${SECRETS_ENDPOINTS.COLLECTION}/${itemId}`, payload)
  return data
}

export async function deleteSecret(itemId: string): Promise<void> {
  await api.delete(`${SECRETS_ENDPOINTS.COLLECTION}/${itemId}`)
}
