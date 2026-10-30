<script setup lang="ts">
import { useAuth } from '@/composables/userAuth'
import { messages } from '@/resources'

const { login, password, loading, errorMessage, handleLogin } = useAuth()
</script>

<template>
  <main class="login-page">
    <section class="card login-card">
      <header class="login-card__header">
        <p class="login-card__brand">{{ messages.admin.appName }}</p>
        <h1 class="login-card__title">{{ messages.auth.loginTitle }}</h1>
      </header>

      <form class="login-form" novalidate @submit.prevent="handleLogin">
        <div class="field">
          <label class="field__label" for="login">{{ messages.auth.loginLabel }}</label>
          <input
            id="login"
            v-model="login"
            class="input"
            type="text"
            name="username"
            autocomplete="username"
            required
            :disabled="loading"
          />
        </div>

        <div class="field">
          <label class="field__label" for="password">{{ messages.auth.passwordLabel }}</label>
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
          {{ loading ? messages.auth.submitLoading : messages.auth.submit }}
        </button>
      </form>
    </section>
  </main>
</template>

<style scoped>
.login-page {
  display: grid;
  place-items: center;
  min-height: 100vh;
  padding: var(--space-5);
}

.login-card {
  width: 100%;
  max-width: 380px;
  padding: var(--space-6);
  box-shadow: var(--shadow-md);
}

.login-card__header {
  margin-bottom: var(--space-5);
}

.login-card__brand {
  margin-bottom: var(--space-2);
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-accent);
}

.login-card__title {
  font-size: var(--font-size-lg);
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.login-form .btn {
  margin-top: var(--space-1);
}
</style>
