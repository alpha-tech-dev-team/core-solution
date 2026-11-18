import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useTranslations } from '@shared/i18n/create-i18n'
import { SHARED_ROUTE_NAMES } from '@shared/constants/routes'
import { useViewError } from '@shared/composables/use-view-error'
import { SECRETS_PAGE_SIZE } from '@/constants/pagination'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { validateSecretForm } from './validate-secret-form'
import { useSecretForm } from './use-secret-form'
import { useSecretsCrud } from './use-secrets-crud'
import { useSecretsList } from './use-secrets-list'

export function useSecretsView() {
  const router = useRouter()
  const { tr, trValidationMessage } = useTranslations()
  const { errorMessage, run } = useViewError()

  const pageSize = ref(SECRETS_PAGE_SIZE)
  const total = ref(0)

  const list = useSecretsList(pageSize, total, run)
  const form = useSecretForm()

  function confirmDelete(): boolean {
    return window.confirm(tr(RESOURCE_KEYS.SECRETS_DELETE_CONFIRM))
  }

  const crud = useSecretsCrud(list, form, run, confirmDelete)

  async function save(): Promise<void> {
    const validationMessage = validateSecretForm({
      name: form.name.value,
      username: form.username.value,
      password: form.password.value,
      isEditing: form.editingId.value !== null,
    })

    if (validationMessage !== null) {
      errorMessage.value = trValidationMessage(validationMessage)
      return
    }

    await crud.save()
  }

  function goHome(): void {
    void router.push({ name: SHARED_ROUTE_NAMES.HOME })
  }

  onMounted(list.load)

  return {
    secrets: list.items,
    page: list.page,
    totalPages: list.totalPages,
    isLoading: list.isLoading,
    isSaving: crud.isSaving,
    isFormVisible: form.isVisible,
    editingId: form.editingId,
    formName: form.name,
    formUsername: form.username,
    formPassword: form.password,
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
    goHome,
  }
}
