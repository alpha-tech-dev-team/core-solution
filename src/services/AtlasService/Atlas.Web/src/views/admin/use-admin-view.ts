import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ROUTE_NAME } from '@/constants/router'

export function useAdminView() {
  const authStore = useAuthStore()
  const router = useRouter()

  async function logout(): Promise<void> {
    await authStore.logout()
    await router.push({ name: ROUTE_NAME.LOGIN })
  }

  return { logout }
}
