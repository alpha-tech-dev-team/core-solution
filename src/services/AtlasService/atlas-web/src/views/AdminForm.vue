<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { RouteName } from '@/constants/router.Constants'
import { messages } from '@/resources'

const authStore = useAuthStore()
const router = useRouter()

const stats = [
  { label: messages.admin.usersLabel, value: messages.admin.emptyValue },
  { label: messages.admin.sessionsLabel, value: messages.admin.emptyValue },
  { label: messages.admin.requestsLabel, value: messages.admin.emptyValue },
]

async function handleLogout() {
  authStore.logout()
  await router.push({ name: RouteName.Login })
}
</script>

<template>
  <div class="admin">
    <header class="admin__topbar">
      <div class="admin__brand">
        <span class="admin__brand-name">{{ messages.admin.appName }}</span>
        <span class="admin__brand-separator"></span>
        <span class="admin__brand-section">{{ messages.admin.sectionTitle }}</span>
      </div>

      <button class="btn btn--ghost btn--sm" type="button" @click="handleLogout">
        {{ messages.admin.logout }}
      </button>
    </header>

    <main class="admin__content">
      <div class="admin__heading">
        <h1>{{ messages.admin.pageTitle }}</h1>
        <p class="text-muted text-sm">{{ messages.admin.pageSubtitle }}</p>
      </div>

      <section class="stat-grid" :aria-label="messages.admin.statsLabel">
        <article v-for="stat in stats" :key="stat.label" class="card stat">
          <p class="stat__label">{{ stat.label }}</p>
          <p class="stat__value numeric">{{ stat.value }}</p>
        </article>
      </section>

      <section class="card panel">
        <header class="panel__header">
          <h2>{{ messages.admin.recentTitle }}</h2>
        </header>

        <p class="panel__empty">{{ messages.admin.emptyState }}</p>
      </section>
    </main>
  </div>
</template>

<style scoped>
.admin {
  min-height: 100vh;
}

.admin__topbar {
  position: sticky;
  top: 0;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-4);
  height: 56px;
  padding: 0 var(--space-5);
  border-bottom: 1px solid var(--color-border);
  background: var(--color-surface);
}

.admin__brand {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  min-width: 0;
}

.admin__brand-name {
  font-weight: 600;
}

.admin__brand-separator {
  width: 1px;
  height: 16px;
  background: var(--color-border-strong);
}

.admin__brand-section {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}

.admin__content {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
  max-width: 1080px;
  margin: 0 auto;
  padding: var(--space-6) var(--space-5);
}

.admin__heading {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.stat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--space-4);
}

.stat {
  padding: var(--space-5);
}

.stat__label {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}

.stat__value {
  margin-top: var(--space-2);
  font-size: 26px;
  font-weight: 600;
  line-height: 1.2;
}

.panel {
  overflow: hidden;
}

.panel__header {
  padding: var(--space-4) var(--space-5);
  border-bottom: 1px solid var(--color-border);
}

.panel__empty {
  padding: var(--space-6) var(--space-5);
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}

@media (max-width: 640px) {
  .admin__topbar {
    padding: 0 var(--space-4);
  }

  .admin__content {
    padding: var(--space-5) var(--space-4);
  }

  .admin__brand-section,
  .admin__brand-separator {
    display: none;
  }
}
</style>
