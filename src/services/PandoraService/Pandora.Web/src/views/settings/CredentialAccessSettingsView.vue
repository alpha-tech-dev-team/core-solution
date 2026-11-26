<script setup lang="ts">
import LogoutButton from '@shared/components/LogoutButton.vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import { LOADING_PLACEHOLDER } from '@shared/constants/ui'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { CREDENTIAL_ACCESS_TTL_OPTIONS } from '@/constants/credential-access'
import { useCredentialAccessSettings } from './use-credential-access-settings'

const page = useCredentialAccessSettings()
const { tr } = useTranslations()

const {
  ttlMinutes,
  updatedAt,
  updatedAtLabel,
  isLoading,
  isSaving,
  isSaved,
  errorMessage,
  save,
  goHome,
} = page
</script>

<template>
  <div class="page">
    <header class="page__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <nav class="page__nav">
        <button class="btn btn--ghost btn--sm" @click="goHome">
          {{ tr(RESOURCE_KEYS.NAV_HOME) }}
        </button>
      </nav>
      <LogoutButton />
    </header>

    <main class="page__body">
      <div class="toolbar">
        <h2>{{ tr(RESOURCE_KEYS.SETTINGS_TITLE) }}</h2>
      </div>

      <form class="card card--padded form" @submit.prevent="save">
        <h3>{{ tr(RESOURCE_KEYS.SETTINGS_CREDENTIAL_ACCESS_TITLE) }}</h3>

        <label class="field">
          <span class="field__label">{{ tr(RESOURCE_KEYS.SETTINGS_CREDENTIAL_ACCESS_TTL) }}</span>
          <select v-model.number="ttlMinutes" class="input" :disabled="isLoading || isSaving">
            <option v-for="option in CREDENTIAL_ACCESS_TTL_OPTIONS" :key="option" :value="option">
              {{ option }} {{ tr(RESOURCE_KEYS.SETTINGS_CREDENTIAL_ACCESS_MINUTES) }}
            </option>
          </select>
        </label>

        <p class="text-muted">{{ tr(RESOURCE_KEYS.SETTINGS_CREDENTIAL_ACCESS_HINT) }}</p>

        <p v-if="updatedAt !== null" class="text-muted">
          {{ tr(RESOURCE_KEYS.SETTINGS_CREDENTIAL_ACCESS_UPDATED_AT, updatedAtLabel) }}
        </p>
        <p v-else class="text-muted">
          {{ tr(RESOURCE_KEYS.SETTINGS_CREDENTIAL_ACCESS_NOT_UPDATED) }}
        </p>

        <div class="form__actions">
          <button type="submit" class="btn btn--primary" :disabled="isLoading || isSaving">
            {{ isSaving ? LOADING_PLACEHOLDER : tr(RESOURCE_KEYS.SETTINGS_SAVE) }}
          </button>
        </div>
      </form>

      <p v-if="errorMessage" class="alert alert--error">{{ errorMessage }}</p>
      <p v-if="isSaved" class="text-muted">{{ tr(RESOURCE_KEYS.SETTINGS_SAVED) }}</p>
      <p v-if="isLoading" class="text-muted">{{ LOADING_PLACEHOLDER }}</p>
    </main>
  </div>
</template>
