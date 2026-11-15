import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ROUTE_NAMES } from '@/constants/routes'

export function useHomeView() {
  const router = useRouter()
  const authStore = useAuthStore()

  async function logout(): Promise<void> {
    await authStore.logout()
    await router.push({ name: ROUTE_NAMES.LOGIN })
  }

  return { logout }
}
