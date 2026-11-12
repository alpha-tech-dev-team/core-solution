import type { SecretItem } from './secret-item'

export interface SecretPage {
  items: SecretItem[]
  page: number
  pageSize: number
  total: number
}
