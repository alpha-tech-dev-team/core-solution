<script setup lang="ts">
import { useTranslations } from '@shared/i18n/create-i18n'
import { LOADING_PLACEHOLDER } from '@shared/constants/ui'
import { FIRST_PAGE } from '@/constants/pagination'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { useSecretsView } from './use-secrets-view'

const page = useSecretsView()
const { tr } = useTranslations()

const {
  secrets,
  page: currentPage,
  totalPages,
  isLoading,
  isSaving,
  isFormVisible,
  editingId,
  formName,
  formUsername,
  formPassword,
  selectedItem,
  selectedId,
  errorMessage,
  changePage,
  openCreateForm,
  closeForm,
  selectRow,
  editSelected,
  deleteSelected,
  save,
  logout,
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
      <button class="btn btn--ghost btn--sm" @click="logout">
        {{ tr(RESOURCE_KEYS.AUTH_LOGOUT) }}
      </button>
    </header>

    <main class="page__body">
      <div class="toolbar">
        <h2>{{ tr(RESOURCE_KEYS.SECRETS_TITLE) }}</h2>
        <div v-if="!isFormVisible" class="toolbar__group">
          <button class="btn btn--primary" @click="openCreateForm">
            {{ tr(RESOURCE_KEYS.SECRETS_ADD) }}
          </button>
          <button class="btn btn--ghost" :disabled="selectedItem === null" @click="editSelected">
            {{ tr(RESOURCE_KEYS.SECRETS_EDIT) }}
          </button>
          <button class="btn btn--danger" :disabled="selectedItem === null" @click="deleteSelected">
            {{ tr(RESOURCE_KEYS.SECRETS_DELETE) }}
          </button>
        </div>
      </div>

      <form v-if="isFormVisible" class="card card--padded form" @submit.prevent="save">
        <label class="field">
          <span class="field__label">{{ tr(RESOURCE_KEYS.SECRETS_FIELD_NAME) }}</span>
          <input v-model="formName" class="input" type="text" />
        </label>
        <label class="field">
          <span class="field__label">{{ tr(RESOURCE_KEYS.SECRETS_FIELD_USERNAME) }}</span>
          <input v-model="formUsername" class="input" type="text" />
        </label>
        <label class="field">
          <span class="field__label">{{ tr(RESOURCE_KEYS.SECRETS_FIELD_PASSWORD) }}</span>
          <input
            v-model="formPassword"
            class="input"
            type="password"
            :placeholder="editingId === null ? '' : tr(RESOURCE_KEYS.SECRETS_PASSWORD_KEEP)"
          />
        </label>
        <div class="form__actions">
          <button type="button" class="btn btn--ghost" @click="closeForm">
            {{ tr(RESOURCE_KEYS.SECRETS_CANCEL) }}
          </button>
          <button type="submit" class="btn btn--primary" :disabled="isSaving">
            {{ isSaving ? LOADING_PLACEHOLDER : tr(RESOURCE_KEYS.SECRETS_SAVE) }}
          </button>
        </div>
      </form>

      <p v-if="errorMessage" class="alert alert--error">{{ errorMessage }}</p>
      <p v-if="isLoading" class="text-muted">{{ LOADING_PLACEHOLDER }}</p>

      <p v-else-if="secrets.length === 0" class="text-muted">
        {{ tr(RESOURCE_KEYS.SECRETS_EMPTY) }}
      </p>

      <table v-else class="table">
        <thead>
          <tr>
            <th>{{ tr(RESOURCE_KEYS.SECRETS_FIELD_NAME) }}</th>
            <th>{{ tr(RESOURCE_KEYS.SECRETS_FIELD_USERNAME) }}</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="item in secrets"
            :key="item.id"
            :class="{ 'table__row--selected': item.id === selectedId }"
            @click="selectRow(item)"
          >
            <td>{{ item.name }}</td>
            <td>{{ item.username }}</td>
          </tr>
        </tbody>
      </table>

      <nav v-if="totalPages > 1" class="pager">
        <button
          class="btn btn--ghost btn--sm"
          :disabled="currentPage <= FIRST_PAGE"
          @click="changePage(currentPage - 1)"
        >
          {{ tr(RESOURCE_KEYS.SECRETS_PAGE_PREV) }}
        </button>
        <span class="pager__label">
          {{ tr(RESOURCE_KEYS.SECRETS_PAGE_INFO, currentPage, totalPages) }}
        </span>
        <button
          class="btn btn--ghost btn--sm"
          :disabled="currentPage >= totalPages"
          @click="changePage(currentPage + 1)"
        >
          {{ tr(RESOURCE_KEYS.SECRETS_PAGE_NEXT) }}
        </button>
      </nav>
    </main>
  </div>
</template>
