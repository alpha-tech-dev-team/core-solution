import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { identityClient } from '@/api/identity-client'
import type { LoginRequest } from '@/api/contracts/login-request'
import { useAuthStore } from '@/stores/auth'
import { ROUTE_NAME } from '@/constants/router'
import { ru } from '@/i18n'
import { loginSchema } from '@/validation/login-schema'

export function useLoginView() {
  const authStore = useAuthStore()
  const router = useRouter()

  const username = ref('')
  const password = ref('')
  const loading = ref(false)
  const errorMessage = ref('')

  async function submit(): Promise<void> {
    if (loading.value) return

    errorMessage.value = ''

    const parsed = loginSchema.safeParse({
      username: username.value,
      password: password.value,
    })

    if (!parsed.success) {
      errorMessage.value = parsed.error.issues[0]?.message ?? ru.auth.errorDefault
      return
    }

    loading.value = true

    try {
      const credentials: LoginRequest = parsed.data
      const session = await identityClient.login(credentials)

      authStore.setSession(session)
      await router.push({ name: ROUTE_NAME.ADMIN })
    } catch (error) {
      errorMessage.value = (error as Error).message || ru.auth.errorDefault
    } finally {
      loading.value = false
    }
  }

  return {
    username,
    password,
    loading,
    errorMessage,
    submit,
  }
}
