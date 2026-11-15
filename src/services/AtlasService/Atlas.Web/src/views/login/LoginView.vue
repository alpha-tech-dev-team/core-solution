<script setup lang="ts">
import { ru } from '@/i18n'
import { useLoginView } from './use-login-view'

const { username, password, loading, errorMessage, submit } = useLoginView()
</script>

<template>
  <main class="login-page">
    <section class="card login-card">
      <header class="login-card__header">
        <p class="login-card__brand">{{ ru.admin.appName }}</p>
        <h1 class="login-card__title">{{ ru.auth.loginTitle }}</h1>
      </header>

      <form class="login-form" novalidate @submit.prevent="submit">
        <div class="field">
          <label class="field__label" for="login">{{ ru.auth.loginLabel }}</label>
          <input
            id="login"
            v-model="username"
            class="input"
            type="text"
            name="username"
            autocomplete="username"
            required
            :disabled="loading"
          />
        </div>

        <div class="field">
          <label class="field__label" for="password">{{ ru.auth.passwordLabel }}</label>
          <input
            id="password"
            v-model="password"
            class="input"
            type="password"
            name="password"
            autocomplete="current-password"
            required
            :disabled="loading"
          />
        </div>

        <p v-if="errorMessage" class="alert alert--error" role="alert">
          {{ errorMessage }}
        </p>

        <button class="btn btn--primary btn--block" type="submit" :disabled="loading">
          {{ loading ? ru.auth.submitLoading : ru.auth.submit }}
        </button>
      </form>
    </section>
  </main>
</template>

<style scoped src="./login-view.css"></style>
