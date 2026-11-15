<script setup lang="ts">
import { ru } from '@/i18n'
import { ADMIN_STATS } from './admin-stats'
import { useAdminView } from './use-admin-view'

const { logout } = useAdminView()
</script>

<template>
  <div class="admin">
    <header class="admin__topbar">
      <div class="admin__brand">
        <span class="admin__brand-name">{{ ru.admin.appName }}</span>
        <span class="admin__brand-separator"></span>
        <span class="admin__brand-section">{{ ru.admin.sectionTitle }}</span>
      </div>

      <button class="btn btn--ghost btn--sm" type="button" @click="logout">
        {{ ru.admin.logout }}
      </button>
    </header>

    <main class="admin__content">
      <div class="admin__heading">
        <h1>{{ ru.admin.pageTitle }}</h1>
        <p class="text-muted text-sm">{{ ru.admin.pageSubtitle }}</p>
      </div>

      <section class="stat-grid" :aria-label="ru.admin.statsLabel">
        <article v-for="stat in ADMIN_STATS" :key="stat.label" class="card stat">
          <p class="stat__label">{{ stat.label }}</p>
          <p class="stat__value numeric">{{ stat.value }}</p>
        </article>
      </section>

      <section class="card panel">
        <header class="panel__header">
          <h2>{{ ru.admin.recentTitle }}</h2>
        </header>

        <p class="panel__empty">{{ ru.admin.emptyState }}</p>
      </section>
    </main>
  </div>
</template>

<style scoped src="./admin-view.css"></style>
