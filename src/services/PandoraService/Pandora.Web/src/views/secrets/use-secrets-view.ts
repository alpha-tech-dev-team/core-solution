import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { tr, trRequestError } from '@/i18n'
import { RESOURCE_KEYS } from '@/constants/i18n'
import { ROUTE_NAMES } from '@/constants/routes'
import { FIRST_PAGE, SECRETS_PAGE_SIZE } from '@/constants/pagination'
import { toRequestError } from '@/api/request-error'
import { createSecret, deleteSecret, fetchSecrets, updateSecret } from '@/api/secrets'
import type { SecretItem } from '@/models/secret-item'
import { validateSecretForm } from './validate-secret-form'

export function useSecretsView() {
  const router = useRouter()
  const authStore = useAuthStore()

  const secrets = ref<SecretItem[]>([])
  const page = ref(FIRST_PAGE)
  const pageSize = ref(SECRETS_PAGE_SIZE)
  const total = ref(0)
  const isLoading = ref(false)
  const isSaving = ref(false)
  const isDeleting = ref(false)
  const errorMessage = ref('')
  const isFormVisible = ref(false)
  const editingId = ref<string | null>(null)
  const formName = ref('')
  const formUsername = ref('')
  const formPassword = ref('')
  const selectedId = ref<string | null>(null)

  const selectedItem = computed(
    () => secrets.value.find((item) => item.id === selectedId.value) ?? null,
  )
  const totalPages = computed(() => Math.max(FIRST_PAGE, Math.ceil(total.value / pageSize.value)))

  async function loadSecrets(): Promise<void> {
    isLoading.value = true
    errorMessage.value = ''

    try {
      let result = await fetchSecrets(page.value, pageSize.value)

      while (result.items.length === 0 && result.total > 0 && page.value > FIRST_PAGE) {
        page.value -= 1
        result = await fetchSecrets(page.value, pageSize.value)
      }

      secrets.value = result.items
      page.value = result.page
      pageSize.value = result.pageSize
      total.value = result.total
    } catch (error) {
      errorMessage.value = trRequestError(toRequestError(error))
    } finally {
      isLoading.value = false
    }
  }

  async function changePage(target: number): Promise<void> {
    if (target < FIRST_PAGE || target > totalPages.value || target === page.value) return

    page.value = target
    selectedId.value = null
    await loadSecrets()
  }

  function resetForm(): void {
    editingId.value = null
    formName.value = ''
    formUsername.value = ''
    formPassword.value = ''
  }

  function openCreateForm(): void {
    resetForm()
    isFormVisible.value = true
  }

  function openEditForm(item: SecretItem): void {
    editingId.value = item.id
    formName.value = item.name
    formUsername.value = item.username
    formPassword.value = ''
    isFormVisible.value = true
  }

  function closeForm(): void {
    isFormVisible.value = false
    resetForm()
  }

  function selectRow(item: SecretItem): void {
    selectedId.value = selectedId.value === item.id ? null : item.id
  }

  function editSelected(): void {
    if (selectedItem.value !== null) openEditForm(selectedItem.value)
  }

  function deleteSelected(): void {
    if (selectedItem.value !== null) void remove(selectedItem.value)
  }

  async function save(): Promise<void> {
    const name = formName.value.trim()
    const username = formUsername.value.trim()

    const validationError = validateSecretForm({
      name,
      username,
      password: formPassword.value,
      isEditing: editingId.value !== null,
    })

    if (validationError !== null) {
      errorMessage.value = validationError
      return
    }

    isSaving.value = true
    errorMessage.value = ''

    try {
      if (editingId.value === null) {
        await createSecret({ name, username, password: formPassword.value })
      } else {
        await updateSecret(editingId.value, {
          name,
          username,
          password: formPassword.value.length === 0 ? null : formPassword.value,
        })
      }

      closeForm()
      await loadSecrets()
    } catch (error) {
      errorMessage.value = trRequestError(toRequestError(error))
    } finally {
      isSaving.value = false
    }
  }

  async function remove(item: SecretItem): Promise<void> {
    if (isDeleting.value) return
    if (!window.confirm(tr(RESOURCE_KEYS.SECRETS_DELETE_CONFIRM))) return

    errorMessage.value = ''
    isDeleting.value = true

    try {
      await deleteSecret(item.id)
      selectedId.value = null
      await loadSecrets()
    } catch (error) {
      errorMessage.value = trRequestError(toRequestError(error))
    } finally {
      isDeleting.value = false
    }
  }

  async function logout(): Promise<void> {
    await authStore.logout()
    await router.push({ name: ROUTE_NAMES.LOGIN })
  }

  function goHome(): void {
    void router.push({ name: ROUTE_NAMES.HOME })
  }

  onMounted(loadSecrets)

  return {
    secrets,
    page,
    totalPages,
    isLoading,
    isSaving,
    isFormVisible,
    editingId,
    formName,
    formUsername,
    formPassword,
    selectedItem,
    selectedId,
    errorMessage,
    changePage,
    openCreateForm,
    openEditForm,
    closeForm,
    selectRow,
    editSelected,
    deleteSelected,
    save,
    logout,
    goHome,
  }
}
