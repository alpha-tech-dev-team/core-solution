import { ref, type Ref } from 'vue'
import type { SecretItem } from '@/models/secret-item'
import type { CreateSecretPayload } from '@/models/create-secret-payload'
import type { UpdateSecretPayload } from '@/models/update-secret-payload'

export interface SecretForm {
  isVisible: Ref<boolean>
  editingId: Ref<string | null>
  name: Ref<string>
  username: Ref<string>
  password: Ref<string>
  openCreate(): void
  openEdit(item: SecretItem): void
  close(): void
  toCreatePayload(): CreateSecretPayload
  toUpdatePayload(): UpdateSecretPayload
}

export function useSecretForm(): SecretForm {
  const isVisible = ref(false)
  const editingId = ref<string | null>(null)
  const name = ref('')
  const username = ref('')
  const password = ref('')

  function reset(): void {
    editingId.value = null
    name.value = ''
    username.value = ''
    password.value = ''
  }

  function toCreatePayload(): CreateSecretPayload {
    return { name: name.value.trim(), username: username.value.trim(), password: password.value }
  }

  function toUpdatePayload(): UpdateSecretPayload {
    const id = editingId.value
    if (id === null) throw new Error('Secret id is missing for update')

    return {
      name: name.value.trim(),
      username: username.value.trim(),
      password: password.value.length === 0 ? null : password.value,
    }
  }

  return {
    isVisible,
    editingId,
    name,
    username,
    password,
    openCreate: () => {
      reset()
      isVisible.value = true
    },
    openEdit: (item) => {
      editingId.value = item.id
      name.value = item.name
      username.value = item.username
      password.value = ''
      isVisible.value = true
    },
    close: () => {
      isVisible.value = false
      reset()
    },
    toCreatePayload,
    toUpdatePayload,
  }
}
