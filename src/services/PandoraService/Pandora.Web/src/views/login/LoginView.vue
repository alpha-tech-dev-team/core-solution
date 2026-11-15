<script setup lang="ts">
import { RESOURCE_KEYS } from '@/constants/i18n'
import { LOADING_PLACEHOLDER } from '@/constants/ui'
import { tr } from '@/i18n'
import { useLoginView } from './use-login-view'

const { username, password, errorMessage, isSubmitting, submit } = useLoginView()
</script>

<template>
  <div class="login">
    <form class="login__card" @submit.prevent="submit">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <label class="login__field">
        <span>{{ tr(RESOURCE_KEYS.FIELD_USERNAME) }}</span>
        <input v-model="username" type="text" autocomplete="username" />
      </label>
      <label class="login__field">
        <span>{{ tr(RESOURCE_KEYS.FIELD_PASSWORD) }}</span>
        <input v-model="password" type="password" autocomplete="current-password" />
      </label>
      <p v-if="errorMessage" class="login__error">{{ errorMessage }}</p>
      <button type="submit" :disabled="isSubmitting">
        {{ isSubmitting ? LOADING_PLACEHOLDER : tr(RESOURCE_KEYS.AUTH_LOGIN) }}
      </button>
    </form>
  </div>
</template>

<style scoped src="./login-view.css"></style>
