import { ref, type Ref } from 'vue'
import type { CreateRoomPayload } from '@/models/create-room-payload'
import type { RoomItem } from '@/models/room-item'
import type { UpdateRoomPayload } from '@/models/update-room-payload'

export interface RoomForm {
  isVisible: Ref<boolean>
  editingId: Ref<string | null>
  name: Ref<string>
  description: Ref<string>
  openCreate(): void
  openEdit(item: RoomItem): void
  close(): void
  toCreatePayload(): CreateRoomPayload
  toUpdatePayload(): UpdateRoomPayload
}

export function useRoomForm(): RoomForm {
  const isVisible = ref(false)
  const editingId = ref<string | null>(null)
  const name = ref('')
  const description = ref('')

  function reset(): void {
    editingId.value = null
    name.value = ''
    description.value = ''
  }

  function toDescription(): string | null {
    const normalized = description.value.trim()

    return normalized.length === 0 ? null : normalized
  }

  function toCreatePayload(): CreateRoomPayload {
    return { name: name.value.trim(), description: toDescription() }
  }

  function toUpdatePayload(): UpdateRoomPayload {
    return { name: name.value.trim(), description: toDescription() }
  }

  return {
    isVisible,
    editingId,
    name,
    description,
    openCreate: () => {
      reset()
      isVisible.value = true
    },
    openEdit: (item) => {
      editingId.value = item.id
      name.value = item.name
      description.value = item.description ?? ''
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
