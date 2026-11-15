import { useRouter } from 'vue-router'
import { ROUTE_NAMES } from '@/constants/routes'
import { useAuthStore } from '@/session'

export function useHomeView() {
  const router = useRouter()
  const authStore = useAuthStore()

  async function logout(): Promise<void> {
    await authStore.logout()
    await router.push({ name: ROUTE_NAMES.LOGIN })
  }

  return { logout }
}
