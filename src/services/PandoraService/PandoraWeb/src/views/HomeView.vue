<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { tr } from '@/i18n'
import { RESOURCE_KEYS } from '@/i18n/keys'
import { ROUTES } from '@/router/constants'

const router = useRouter()
const authStore = useAuthStore()

function logout() {
  authStore.logout()
  router.push(ROUTES.LOGIN)
}
</script>

<template>
  <div class="home">
    <header class="home__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <button class="home__logout" @click="logout">{{ tr(RESOURCE_KEYS.AUTH_LOGOUT) }}</button>
    </header>
    <main class="home__body">
      <p>{{ tr(RESOURCE_KEYS.APP_PLACEHOLDER) }}</p>
    </main>
  </div>
</template>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.home__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-3) var(--space-5);
  background: var(--color-surface);
  border-bottom: var(--border-width) solid var(--color-border);
}

.home__header h1 {
  font-size: var(--font-size-heading);
}

.home__logout {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.home__logout:hover {
  background: var(--color-bg);
}

.home__body {
  display: flex;
  flex: 1;
  align-items: center;
  justify-content: center;
  font-size: var(--font-size-large);
  color: var(--color-text-muted);
}
</style>