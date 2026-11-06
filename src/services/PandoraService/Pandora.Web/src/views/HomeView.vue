<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { tr } from '@/i18n'
import { ROUTES } from '@/constants/routes'

const router = useRouter()
const authStore = useAuthStore()

async function logout() {
  await authStore.logout()
  router.push(ROUTES.LOGIN)
}
</script>

<template>
  <div class="home">
    <header class="home__header">
      <h1>{{ tr('app_title') }}</h1>
      <nav class="home__nav">
        <RouterLink class="home__link" :to="ROUTES.EMPLOYEES">{{ tr('employees_title') }}</RouterLink>
        <button class="home__logout" @click="logout">{{ tr('auth_logout') }}</button>
      </nav>
    </header>
    <main class="home__body"></main>
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

.home__nav {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.home__logout,
.home__link {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  color: var(--color-text);
  font-size: var(--font-size-base);
  text-decoration: none;
}

.home__logout:hover,
.home__link:hover {
  background: var(--color-bg);
}

.home__body {
  flex: 1;
}
</style>
