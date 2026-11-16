import { api } from '@shared/app/app-session'
import { PAGINATION_QUERY, SECRET_ENDPOINTS } from '@/constants/api'
import type { CreateSecretPayload } from '@/models/create-secret-payload'
import type { SecretItem } from '@/models/secret-item'
import type { SecretSource } from '@/models/secret-source'
import type { Page } from '@shared/models/page'
import type { UpdateSecretPayload } from '@/models/update-secret-payload'

export async function fetchSecrets(
  page: number,
  pageSize: number,
  source: SecretSource,
): Promise<Page<SecretItem>> {
  return await api.get<Page<SecretItem>>(SECRET_ENDPOINTS.COLLECTION, {
    [PAGINATION_QUERY.PAGE]: page,
    [PAGINATION_QUERY.PAGE_SIZE]: pageSize,
    [PAGINATION_QUERY.SOURCE]: source,
  })
}

export async function createSecret(payload: CreateSecretPayload): Promise<SecretItem> {
  return await api.post<SecretItem>(SECRET_ENDPOINTS.COLLECTION, payload)
}

export async function updateSecret(
  secretId: string,
  payload: UpdateSecretPayload,
): Promise<SecretItem> {
  return await api.patch<SecretItem>(SECRET_ENDPOINTS.BY_ID(secretId), payload)
}

export async function deleteSecret(secretId: string): Promise<void> {
  await api.delete(SECRET_ENDPOINTS.BY_ID(secretId))
}
