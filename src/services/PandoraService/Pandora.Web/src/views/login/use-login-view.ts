import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { trRequestError } from '@/i18n'
import { ROUTE_NAMES } from '@/constants/routes'
import { validateCredentials } from './validate-credentials'

export function useLoginView() {
  const router = useRouter()
  const authStore = useAuthStore()

  const username = ref('')
  const password = ref('')
  const errorMessage = ref('')
  const isSubmitting = ref(false)

  async function submit(): Promise<void> {
    errorMessage.value = ''

    const validationError = validateCredentials({
      username: username.value,
      password: password.value,
    })

    if (validationError !== null) {
      errorMessage.value = validationError
      return
    }

    isSubmitting.value = true
    const loginError = await authStore.login(username.value, password.value)
    isSubmitting.value = false

    if (loginError === null) {
      await router.push({ name: ROUTE_NAMES.HOME })
      return
    }

    errorMessage.value = trRequestError(loginError)
  }

  return {
    username,
    password,
    errorMessage,
    isSubmitting,
    submit,
  }
}
