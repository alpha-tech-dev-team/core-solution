import { ref } from 'vue'
import { identityClient } from '@/api/identityClient'
import type { LoginRequest } from '@/api/contracts/LoginRequest'
import { useAuthStore } from '@/stores/authStore'
import { RouteName } from '@/constants/router.Constants'
import { Atlas } from '@/resources'
import { useRouter } from 'vue-router'

export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()

  const login = ref('')
  const password = ref('')
  const loading = ref(false)
  const errorMessage = ref('')

  const handleLogin = async () => {
    if (loading.value) return

    errorMessage.value = ''

    const username = login.value.trim()

    if (!username || !password.value) {
      errorMessage.value = Atlas.ru.auth.errorRequired
      return
    }

    loading.value = true

    try {
      const credentials: LoginRequest = {
        username,
        password: password.value,
      }

      const session = await identityClient.login(credentials)
      authStore.setSession(session)
      await router.push({ name: RouteName.Admin })
    } catch (e: unknown) {
      const err = e as Error
      errorMessage.value = err.message || Atlas.ru.auth.errorDefault
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
