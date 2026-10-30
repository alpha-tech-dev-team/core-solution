import { ref } from 'vue'
import { identityClient } from '@/api/identityClient'
import type { LoginRequest } from '@/api/contracts'
import { useAuthStore } from '@/stores/authStore'
import { RouteName } from '@/constants/router.Constants'
import { messages } from '@/resources'
import { useRouter } from 'vue-router'

export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()

  const login = ref('')
  const password = ref('')
  const loading = ref(false)
  const errorMessage = ref('')

  const handleLogin = async () => {
    loading.value = true
    errorMessage.value = ''

    try {
      const credentials: LoginRequest = {
        username: login.value,
        password: password.value,
      }

      const session = await identityClient.login(credentials)
      authStore.setSession(session)
      await router.push({ name: RouteName.Admin })
    } catch (e: unknown) {
      const err = e as Error
      errorMessage.value = err.message || messages.auth.errorDefault
    } finally {
      loading.value = false
    }
  }

  return {
    login,
    password,
    loading,
    errorMessage,
    handleLogin,
  }
}
