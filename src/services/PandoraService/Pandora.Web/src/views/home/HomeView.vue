<script setup lang="ts">
import LogoutButton from '@shared/components/LogoutButton.vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { ROUTE_NAMES } from '@/constants/routes'

const { tr } = useTranslations()
</script>

<template>
  <div class="page">
    <header class="page__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <nav class="page__nav">
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.EMPLOYEES }">
          {{ tr(RESOURCE_KEYS.EMPLOYEES_TITLE) }}
        </RouterLink>
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.SECRETS }">
          {{ tr(RESOURCE_KEYS.NAV_SECRETS) }}
        </RouterLink>
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.SETTINGS }">
          {{ tr(RESOURCE_KEYS.NAV_SETTINGS) }}
        </RouterLink>
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.AUDIT }">
          {{ tr(RESOURCE_KEYS.NAV_AUDIT) }}
        </RouterLink>
        <LogoutButton />
      </nav>
    </header>

    <main class="page__body"></main>
  </div>
</template>
