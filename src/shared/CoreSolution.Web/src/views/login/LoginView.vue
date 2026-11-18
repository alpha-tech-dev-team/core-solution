<script setup lang="ts">
import { SHARED_RESOURCE_KEYS } from '../../constants/resource-keys'
import { LOADING_PLACEHOLDER } from '../../constants/ui'
import { useTranslations } from '../../i18n/create-i18n'
import { useLoginView } from './use-login-view'

const { username, password, errorMessage, isSubmitting, submit } = useLoginView()
const { tr } = useTranslations()
</script>

<template>
  <main class="login-page">
    <section class="card login-card">
      <header class="login-card__header">
        <p class="login-card__brand">
          {{ tr(SHARED_RESOURCE_KEYS.APP_TITLE) }}
        </p>
        <h1 class="login-card__title">
          {{ tr(SHARED_RESOURCE_KEYS.AUTH_LOGIN_TITLE) }}
        </h1>
      </header>

      <form class="login-form" novalidate @submit.prevent="submit">
        <div class="field">
          <label class="field__label" for="login">{{
            tr(SHARED_RESOURCE_KEYS.FIELD_USERNAME)
          }}</label>
          <input
            id="login"
            v-model="username"
            class="input"
            type="text"
            name="username"
            autocomplete="username"
            required
            :disabled="isSubmitting"
          />
        </div>

        <div class="field">
          <label class="field__label" for="password">
            {{ tr(SHARED_RESOURCE_KEYS.FIELD_PASSWORD) }}
          </label>
          <input
            id="password"
            v-model="password"
            class="input"
            type="password"
            name="password"
            autocomplete="current-password"
            required
            :disabled="isSubmitting"
          />
        </div>

        <p v-if="errorMessage" class="alert alert--error" role="alert">
          {{ errorMessage }}
        </p>

        <button class="btn btn--primary btn--block" type="submit" :disabled="isSubmitting">
          {{ isSubmitting ? LOADING_PLACEHOLDER : tr(SHARED_RESOURCE_KEYS.AUTH_LOGIN) }}
        </button>
      </form>
    </section>
  </main>
</template>

<style scoped src="./login-view.css"></style>
