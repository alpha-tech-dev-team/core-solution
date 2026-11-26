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
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.INCIDENTS }">
          {{ tr(RESOURCE_KEYS.NAV_INCIDENTS) }}
        </RouterLink>
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.ROOMS }">
          {{ tr(RESOURCE_KEYS.NAV_ROOMS) }}
        </RouterLink>
        <LogoutButton />
      </nav>
    </header>

    <main class="page__body"></main>
  </div>
</template>
