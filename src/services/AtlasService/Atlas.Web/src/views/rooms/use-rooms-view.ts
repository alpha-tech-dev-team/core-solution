import { onMounted, ref } from 'vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import { useViewError } from '@shared/composables/use-view-error'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { ROOMS_PAGE_SIZE } from '@/constants/pagination'
import { validateRoomForm } from './validate-room-form'
import { useRoomForm } from './use-room-form'
import { useRoomsCrud } from './use-rooms-crud'
import { useRoomsList } from './use-rooms-list'

export function useRoomsView() {
  const { tr, trValidationMessage } = useTranslations()
  const { errorMessage, run } = useViewError()

  const pageSize = ref(ROOMS_PAGE_SIZE)
  const total = ref(0)

  const list = useRoomsList(pageSize, total, run)
  const form = useRoomForm()

  function confirmDelete(): boolean {
    return window.confirm(tr(RESOURCE_KEYS.ROOM_DELETE_CONFIRM))
  }

  const crud = useRoomsCrud(list, form, run, confirmDelete)

  async function save(): Promise<void> {
    const validationMessage = validateRoomForm({
      name: form.name.value,
      description: form.description.value,
    })

    if (validationMessage !== null) {
      errorMessage.value = trValidationMessage(validationMessage)
      return
    }

    await crud.save()
  }

  onMounted(list.load)

  return {
    rooms: list.items,
    page: list.page,
    totalPages: list.totalPages,
    isLoading: list.isLoading,
    isSaving: crud.isSaving,
    isFormVisible: form.isVisible,
    editingId: form.editingId,
    formName: form.name,
    formDescription: form.description,
    selectedItem: list.selectedItem,
    selectedId: list.selectedId,
    errorMessage,
    changePage: list.changePage,
    openCreateForm: form.openCreate,
    closeForm: form.close,
    selectRow: list.select,
    editSelected: crud.editSelected,
    deleteSelected: crud.deleteSelected,
    save,
  }
}
