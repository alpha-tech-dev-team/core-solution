import api from './client'
import { PAGINATION_QUERY, SECRET_ENDPOINTS } from '@/constants/api'
import type { CreateSecretPayload } from '@/models/create-secret-payload'
import type { SecretItem } from '@/models/secret-item'
import type { SecretPage } from '@/models/secret-page'
import type { UpdateSecretPayload } from '@/models/update-secret-payload'

export async function fetchSecrets(page: number, pageSize: number): Promise<SecretPage> {
  const { data } = await api.get<SecretPage>(SECRET_ENDPOINTS.COLLECTION, {
    params: {
      [PAGINATION_QUERY.PAGE]: page,
      [PAGINATION_QUERY.PAGE_SIZE]: pageSize,
    },
  })

  return data
}

export async function createSecret(payload: CreateSecretPayload): Promise<SecretItem> {
  const { data } = await api.post<SecretItem>(SECRET_ENDPOINTS.COLLECTION, payload)
  return data
}

export async function updateSecret(
  itemId: string,
  payload: UpdateSecretPayload,
): Promise<SecretItem> {
  const { data } = await api.patch<SecretItem>(SECRET_ENDPOINTS.BY_ID(itemId), payload)
  return data
}

export async function deleteSecret(itemId: string): Promise<void> {
  await api.delete(SECRET_ENDPOINTS.BY_ID(itemId))
}
