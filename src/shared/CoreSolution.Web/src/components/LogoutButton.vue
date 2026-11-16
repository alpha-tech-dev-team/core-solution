<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useTranslations } from '../i18n/create-i18n'
import { useSession } from '../auth/session'
import { SHARED_RESOURCE_KEYS } from '../constants/resource-keys'

const router = useRouter()
const { useAuthStore, loginRouteName } = useSession()
const { tr } = useTranslations()

async function logout(): Promise<void> {
  await useAuthStore().logout()
  await router.push({ name: loginRouteName })
}
</script>

<template>
  <button class="btn btn--ghost btn--sm" type="button" @click="logout">
    {{ tr(SHARED_RESOURCE_KEYS.AUTH_LOGOUT) }}
  </button>
</template>
