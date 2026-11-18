import { ref, type Ref } from 'vue'
import { createSecret, deleteSecret, updateSecret } from '@/api/secrets'
import type { SecretItem } from '@/models/secret-item'
import type { SecretsList } from './use-secrets-list'
import type { SecretForm } from './use-secret-form'

export interface SecretsCrud {
  isSaving: Ref<boolean>
  save(): Promise<void>
  editSelected(): void
  deleteSelected(): void
}

export function useSecretsCrud(
  list: SecretsList,
  form: SecretForm,
  run: (action: () => Promise<void>) => Promise<boolean>,
  confirmDelete: () => boolean,
): SecretsCrud {
  const isSaving = ref(false)
  const isDeleting = ref(false)

  async function save(): Promise<void> {
    isSaving.value = true

    try {
      const isSaved = await run(async () => {
        const editingId = form.editingId.value
        if (editingId === null) {
          await createSecret(form.toCreatePayload())
          return
        }

        await updateSecret(editingId, form.toUpdatePayload())
      })

      if (!isSaved) return

      form.close()
      await list.load()
    } finally {
      isSaving.value = false
    }
  }

  async function removeItem(item: SecretItem): Promise<void> {
    if (isDeleting.value) return
    if (!confirmDelete()) return

    isDeleting.value = true

    try {
      await run(async () => {
        await deleteSecret(item.id)
        list.resetSelection()
        await list.load()
      })
    } finally {
      isDeleting.value = false
    }
  }

  return {
    isSaving,
    save,
    editSelected: () => {
      const item = list.selectedItem.value
      if (item !== null) form.openEdit(item)
    },
    deleteSelected: () => {
      const item = list.selectedItem.value
      if (item !== null) void removeItem(item)
    },
  }
}
