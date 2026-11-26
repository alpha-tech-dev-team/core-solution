import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { SHARED_ROUTE_NAMES } from '@shared/constants/routes'
import { useViewError } from '@shared/composables/use-view-error'
import {
  fetchCredentialAccessSettings,
  updateCredentialAccessSettings,
} from '@/api/credential-access-settings'
import { CREDENTIAL_ACCESS_DEFAULT_TTL_MINUTES } from '@/constants/credential-access'

export function useCredentialAccessSettings() {
  const router = useRouter()
  const { errorMessage, run } = useViewError()

  const ttlMinutes = ref(CREDENTIAL_ACCESS_DEFAULT_TTL_MINUTES)
  const updatedAt = ref<string | null>(null)
  const isLoading = ref(true)
  const isSaving = ref(false)
  const isSaved = ref(false)

  const updatedAtLabel = computed(() =>
    updatedAt.value === null ? '' : new Date(updatedAt.value).toLocaleString(),
  )

  async function load(): Promise<void> {
    isLoading.value = true

    await run(async () => {
      const settings = await fetchCredentialAccessSettings()

      ttlMinutes.value = settings.ttlMinutes
      updatedAt.value = settings.updatedAt
    })

    isLoading.value = false
  }

  async function save(): Promise<void> {
    isSaving.value = true
    isSaved.value = false

    const succeeded = await run(async () => {
      const settings = await updateCredentialAccessSettings(ttlMinutes.value)

      updatedAt.value = settings.updatedAt
    })

    isSaving.value = false
    isSaved.value = succeeded
  }

  function goHome(): void {
    void router.push({ name: SHARED_ROUTE_NAMES.HOME })
  }

  onMounted(load)

  return {
    ttlMinutes,
    updatedAt,
    updatedAtLabel,
    isLoading,
    isSaving,
    isSaved,
    errorMessage,
    load,
    save,
    goHome,
  }
}
