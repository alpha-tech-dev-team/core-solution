import { computed } from 'vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import { RESOURCE_KEYS } from '@/constants/resource-keys'

export function useAdminView() {
  const { tr } = useTranslations()

  const stats = computed(() => [
    { label: tr(RESOURCE_KEYS.ADMIN_USERS_LABEL), value: tr(RESOURCE_KEYS.ADMIN_EMPTY_VALUE) },
    { label: tr(RESOURCE_KEYS.ADMIN_SESSIONS_LABEL), value: tr(RESOURCE_KEYS.ADMIN_EMPTY_VALUE) },
    { label: tr(RESOURCE_KEYS.ADMIN_REQUESTS_LABEL), value: tr(RESOURCE_KEYS.ADMIN_EMPTY_VALUE) },
  ])

  return { stats }
}
