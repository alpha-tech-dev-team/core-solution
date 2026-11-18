<script setup lang="ts">
import LogoutButton from '@shared/components/LogoutButton.vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { useAdminView } from './use-admin-view'

const { stats } = useAdminView()
const { tr } = useTranslations()
</script>

<template>
  <div class="page">
    <header class="page__header">
      <div class="admin__brand">
        <span class="admin__brand-name">{{ tr(RESOURCE_KEYS.APP_TITLE) }}</span>
        <span class="admin__brand-separator"></span>
        <span class="admin__brand-section text-sm text-muted">
          {{ tr(RESOURCE_KEYS.ADMIN_SECTION_TITLE) }}
        </span>
      </div>

      <LogoutButton />
    </header>

    <main class="page__body">
      <div class="admin__heading">
        <h1>{{ tr(RESOURCE_KEYS.ADMIN_PAGE_TITLE) }}</h1>
        <p class="text-muted text-sm">{{ tr(RESOURCE_KEYS.ADMIN_PAGE_SUBTITLE) }}</p>
      </div>

      <section class="stat-grid" :aria-label="tr(RESOURCE_KEYS.ADMIN_STATS_LABEL)">
        <article v-for="stat in stats" :key="stat.label" class="card stat">
          <p class="stat__label text-sm text-muted">{{ stat.label }}</p>
          <p class="stat__value numeric">{{ stat.value }}</p>
        </article>
      </section>

      <section class="card panel">
        <header class="panel__header">
          <h2>{{ tr(RESOURCE_KEYS.ADMIN_RECENT_TITLE) }}</h2>
        </header>

        <p class="panel__empty text-sm text-muted">{{ tr(RESOURCE_KEYS.ADMIN_EMPTY_STATE) }}</p>
      </section>
    </main>
  </div>
</template>

<style scoped src="./admin-view.css"></style>
