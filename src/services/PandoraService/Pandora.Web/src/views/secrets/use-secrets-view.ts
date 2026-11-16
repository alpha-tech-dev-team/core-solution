import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useTranslations } from '@shared/i18n/create-i18n'
import { useViewError } from '@/composables/use-view-error'
import { SHARED_ROUTE_NAMES } from '@shared/constants/routes'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { FIRST_PAGE, SECRETS_PAGE_SIZE } from '@/constants/pagination'
import { createSecret, deleteSecret, fetchSecrets, updateSecret } from '@/api/secrets'
import type { SecretItem } from '@/models/secret-item'
import { validateSecretForm } from './validate-secret-form'

export function useSecretsView() {
  const router = useRouter()
  const { tr, trValidationMessage } = useTranslations()
  const { errorMessage, run } = useViewError()

  const secrets = ref<SecretItem[]>([])
  const page = ref(FIRST_PAGE)
  const pageSize = ref(SECRETS_PAGE_SIZE)
  const total = ref(0)
  const isLoading = ref(false)
  const isSaving = ref(false)
  const isDeleting = ref(false)
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

    try {
      await run(async () => {
        let result = await fetchSecrets(page.value, pageSize.value)

        while (result.items.length === 0 && result.total > 0 && page.value > FIRST_PAGE) {
          page.value -= 1
          result = await fetchSecrets(page.value, pageSize.value)
        }

        secrets.value = result.items
        page.value = result.page
        pageSize.value = result.pageSize
        total.value = result.total
      })
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

    const validationMessage = validateSecretForm({
      name,
      username,
      password: formPassword.value,
      isEditing: editingId.value !== null,
    })

    if (validationMessage !== null) {
      errorMessage.value = trValidationMessage(validationMessage)
      return
    }

    isSaving.value = true

    try {
      const isSaved = await run(async () => {
        if (editingId.value === null) {
          await createSecret({ name, username, password: formPassword.value })
          return
        }

        await updateSecret(editingId.value, {
          name,
          username,
          password: formPassword.value.length === 0 ? null : formPassword.value,
        })
      })

      if (!isSaved) return

      closeForm()
      await loadSecrets()
    } finally {
      isSaving.value = false
    }
  }

  async function remove(item: SecretItem): Promise<void> {
    if (isDeleting.value) return
    if (!window.confirm(tr(RESOURCE_KEYS.SECRETS_DELETE_CONFIRM))) return

    isDeleting.value = true

    try {
      await run(async () => {
        await deleteSecret(item.id)
        selectedId.value = null
        await loadSecrets()
      })
    } finally {
      isDeleting.value = false
    }
  }

  function goHome(): void {
    void router.push({ name: SHARED_ROUTE_NAMES.HOME })
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
    closeForm,
    selectRow,
    editSelected,
    deleteSelected,
    save,
    goHome,
  }
}
