import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useTranslations } from '@shared/i18n/create-i18n'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { ROUTE_NAMES } from '@/constants/routes'
import { useAuthStore } from '@/session'

export function useAdminView() {
  const router = useRouter()
  const authStore = useAuthStore()
  const { tr } = useTranslations()

  const stats = computed(() => [
    { label: tr(RESOURCE_KEYS.ADMIN_USERS_LABEL), value: tr(RESOURCE_KEYS.ADMIN_EMPTY_VALUE) },
    { label: tr(RESOURCE_KEYS.ADMIN_SESSIONS_LABEL), value: tr(RESOURCE_KEYS.ADMIN_EMPTY_VALUE) },
    { label: tr(RESOURCE_KEYS.ADMIN_REQUESTS_LABEL), value: tr(RESOURCE_KEYS.ADMIN_EMPTY_VALUE) },
  ])

  async function logout(): Promise<void> {
    await authStore.logout()
    await router.push({ name: ROUTE_NAMES.LOGIN })
  }

  return { stats, logout }
}
