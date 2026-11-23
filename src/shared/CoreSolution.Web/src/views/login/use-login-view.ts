import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useSession } from '../../auth/session'
import { useTranslations } from '../../i18n/create-i18n'
import { credentialsSchema } from '../../validation/credentials'
import { firstValidationMessage } from '../../validation/validation-message'

export function useLoginView() {
  const router = useRouter()
  const { useAuthStore, authenticatedRouteName } = useSession()
  const { trError, trValidationMessage } = useTranslations()
  const authStore = useAuthStore()

  const username = ref('')
  const password = ref('')
  const errorMessage = ref('')
  const isSubmitting = ref(false)

  async function submit(): Promise<void> {
    if (isSubmitting.value) return

    errorMessage.value = ''

    const parsed = credentialsSchema.safeParse({
      username: username.value,
      password: password.value,
    })

    if (!parsed.success) {
      errorMessage.value = trValidationMessage(firstValidationMessage(parsed.error))
      return
    }

    isSubmitting.value = true

    try {
      await authStore.login(parsed.data)
      await router.push({ name: authenticatedRouteName })
    } catch (error) {
      errorMessage.value = trError(error)
    } finally {
      isSubmitting.value = false
    }
  }

  return {
    username,
    password,
    errorMessage,
    isSubmitting,
    submit,
  }
}
