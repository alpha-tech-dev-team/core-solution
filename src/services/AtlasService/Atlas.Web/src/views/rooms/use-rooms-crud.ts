import { ref, type Ref } from 'vue'
import { createRoom, deleteRoom, updateRoom } from '@/api/rooms'
import type { RoomItem } from '@/models/room-item'
import type { RoomForm } from './use-room-form'
import type { RoomsList } from './use-rooms-list'

export interface RoomsCrud {
  isSaving: Ref<boolean>
  save(): Promise<void>
  editSelected(): void
  deleteSelected(): void
}

export function useRoomsCrud(
  list: RoomsList,
  form: RoomForm,
  run: (action: () => Promise<void>) => Promise<boolean>,
  confirmDelete: () => boolean,
): RoomsCrud {
  const isSaving = ref(false)
  const isDeleting = ref(false)

  async function save(): Promise<void> {
    isSaving.value = true

    try {
      const isSaved = await run(async () => {
        const editingId = form.editingId.value

        if (editingId === null) {
          await createRoom(form.toCreatePayload())
          return
        }

        await updateRoom(editingId, form.toUpdatePayload())
      })

      if (!isSaved) return

      form.close()
      await list.load()
    } finally {
      isSaving.value = false
    }
  }

  async function removeItem(item: RoomItem): Promise<void> {
    if (isDeleting.value) return
    if (!confirmDelete()) return

    isDeleting.value = true

    try {
      await run(async () => {
        await deleteRoom(item.id)
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
