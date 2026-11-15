<script setup lang="ts">
import { RESOURCE_KEYS } from '@/constants/i18n'
import { FIRST_PAGE } from '@/constants/pagination'
import { LOADING_PLACEHOLDER } from '@/constants/ui'
import { tr } from '@/i18n'
import { useSecretsView } from './use-secrets-view'

const {
  secrets,
  page,
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
  openEditForm,
  closeForm,
  selectRow,
  editSelected,
  deleteSelected,
  save,
  logout,
  goHome,
} = useSecretsView()
</script>

<template>
  <div class="secrets">
    <header class="secrets__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <nav class="secrets__nav">
        <button class="secrets__nav-link" @click="goHome">
          {{ tr(RESOURCE_KEYS.NAV_HOME) }}
        </button>
        <span class="secrets__nav-link secrets__nav-link--active">
          {{ tr(RESOURCE_KEYS.NAV_SECRETS) }}
        </span>
      </nav>
      <button class="secrets__logout" @click="logout">{{ tr(RESOURCE_KEYS.AUTH_LOGOUT) }}</button>
    </header>

    <main class="secrets__body">
      <div class="secrets__toolbar">
        <h2>{{ tr(RESOURCE_KEYS.SECRETS_TITLE) }}</h2>
        <div v-if="!isFormVisible" class="secrets__toolbar-actions">
          <button class="secrets__add" @click="openCreateForm">
            {{ tr(RESOURCE_KEYS.SECRETS_ADD) }}
          </button>
          <button class="secrets__button" :disabled="selectedItem === null" @click="editSelected">
            {{ tr(RESOURCE_KEYS.SECRETS_EDIT) }}
          </button>
          <button
            class="secrets__button secrets__button--danger"
            :disabled="selectedItem === null"
            @click="deleteSelected"
          >
            {{ tr(RESOURCE_KEYS.SECRETS_DELETE) }}
          </button>
        </div>
      </div>

      <form v-if="isFormVisible" class="secrets__form" @submit.prevent="save">
        <label class="secrets__field">
          <span>{{ tr(RESOURCE_KEYS.SECRETS_FIELD_NAME) }}</span>
          <input v-model="formName" type="text" />
        </label>
        <label class="secrets__field">
          <span>{{ tr(RESOURCE_KEYS.SECRETS_FIELD_USERNAME) }}</span>
          <input v-model="formUsername" type="text" />
        </label>
        <label class="secrets__field">
          <span>{{ tr(RESOURCE_KEYS.SECRETS_FIELD_PASSWORD) }}</span>
          <input
            v-model="formPassword"
            type="password"
            :placeholder="editingId === null ? '' : tr(RESOURCE_KEYS.SECRETS_PASSWORD_KEEP)"
          />
        </label>
        <div class="secrets__form-actions">
          <button
            type="button"
            class="secrets__button secrets__button--secondary"
            @click="closeForm"
          >
            {{ tr(RESOURCE_KEYS.SECRETS_CANCEL) }}
          </button>
          <button type="submit" class="secrets__button" :disabled="isSaving">
            {{ isSaving ? LOADING_PLACEHOLDER : tr(RESOURCE_KEYS.SECRETS_SAVE) }}
          </button>
        </div>
      </form>

      <p v-if="errorMessage" class="secrets__error">{{ errorMessage }}</p>
      <p v-if="isLoading" class="secrets__loading">{{ LOADING_PLACEHOLDER }}</p>

      <template v-else-if="secrets.length === 0">
        <p class="secrets__empty">{{ tr(RESOURCE_KEYS.SECRETS_EMPTY) }}</p>
      </template>

      <table v-else class="secrets__table">
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
            class="secrets__row"
            :class="{ 'secrets__row--selected': item.id === selectedId }"
            @click="selectRow(item)"
          >
            <td>{{ item.name }}</td>
            <td>{{ item.username }}</td>
          </tr>
        </tbody>
      </table>

      <div v-if="totalPages > 1" class="secrets__pagination">
        <button
          class="secrets__button secrets__button--secondary"
          :disabled="page <= FIRST_PAGE"
          @click="changePage(page - 1)"
        >
          {{ tr(RESOURCE_KEYS.SECRETS_PAGE_PREV) }}
        </button>
        <span class="secrets__pagination-info">
          {{ tr(RESOURCE_KEYS.SECRETS_PAGE_INFO, page, totalPages) }}
        </span>
        <button
          class="secrets__button secrets__button--secondary"
          :disabled="page >= totalPages"
          @click="changePage(page + 1)"
        >
          {{ tr(RESOURCE_KEYS.SECRETS_PAGE_NEXT) }}
        </button>
      </div>
    </main>
  </div>
</template>

<style scoped src="./secrets-view.css"></style>
