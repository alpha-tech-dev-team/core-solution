<script setup lang="ts">
import { RESOURCE_KEYS } from '@/constants/i18n'
import { ROUTE_NAMES } from '@/constants/routes'
import { tr } from '@/i18n'
import { useHomeView } from './use-home-view'

const { logout } = useHomeView()
</script>

<template>
  <div class="home">
    <header class="home__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <nav class="home__nav">
        <RouterLink class="home__link" :to="{ name: ROUTE_NAMES.EMPLOYEES }">
          {{ tr(RESOURCE_KEYS.EMPLOYEES_TITLE) }}
        </RouterLink>
        <RouterLink class="home__link" :to="{ name: ROUTE_NAMES.SECRETS }">
          {{ tr(RESOURCE_KEYS.NAV_SECRETS) }}
        </RouterLink>
        <button class="home__logout" @click="logout">{{ tr(RESOURCE_KEYS.AUTH_LOGOUT) }}</button>
      </nav>
    </header>
    <main class="home__body"></main>
  </div>
</template>

<style scoped src="./home-view.css"></style>
