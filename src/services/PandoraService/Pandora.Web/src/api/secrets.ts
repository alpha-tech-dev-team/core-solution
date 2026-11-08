import api from './client'
import { PAGINATION_QUERY, SECRETS_ENDPOINTS } from '@/constants/api'
import type { SecretItem } from '@/stores/models/secret-item'
import type { SecretPage } from '@/stores/models/secret-page'
import type { CreateSecretPayload } from '@/stores/models/create-secret-payload'
import type { UpdateSecretPayload } from '@/stores/models/update-secret-payload'

export async function fetchSecrets(page: number, pageSize: number): Promise<SecretPage> {
  const { data } = await api.get<SecretPage>(SECRETS_ENDPOINTS.COLLECTION, {
    params: {
      [PAGINATION_QUERY.PAGE]: page,
      [PAGINATION_QUERY.PAGE_SIZE]: pageSize
    }
  })
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
