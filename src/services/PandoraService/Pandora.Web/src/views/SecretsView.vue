<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { tr, trRequestError } from '@/i18n'
import { RESOURCE_KEYS } from '@/constants/i18n'
import { ROUTES } from '@/constants/routes'
import { LOADING_PLACEHOLDER } from '@/constants/ui'
import { FIRST_PAGE, SECRETS_PAGE_SIZE } from '@/constants/pagination'
import { toRequestError } from '@/api/request-error'
import { validateSecretForm } from './secrets/validate-secret-form'
import {
  createSecret,
  deleteSecret,
  fetchSecrets,
  updateSecret
} from '@/api/secrets'
import type { SecretItem } from '@/stores/models/secret-item'

const router = useRouter()
const authStore = useAuthStore()

const secrets = ref<SecretItem[]>([])
const page = ref(FIRST_PAGE)
const pageSize = ref(SECRETS_PAGE_SIZE)
const total = ref(0)
const isLoading = ref(false)
const isSaving = ref(false)
const isDeleting = ref(false)
const errorMessage = ref('')
const isFormVisible = ref(false)
const editingId = ref<string | null>(null)
const formName = ref('')
const formUsername = ref('')
const formPassword = ref('')
const selectedId = ref<string | null>(null)

const selectedItem = computed(() => secrets.value.find((item) => item.id === selectedId.value) ?? null)
const totalPages = computed(() => Math.max(FIRST_PAGE, Math.ceil(total.value / pageSize.value)))

async function loadSecrets() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    let result = await fetchSecrets(page.value, pageSize.value)

    while (result.items.length === 0 && result.total > 0 && page.value > FIRST_PAGE) {
      page.value -= 1
      result = await fetchSecrets(page.value, pageSize.value)
    }

    secrets.value = result.items
    page.value = result.page
    pageSize.value = result.pageSize
    total.value = result.total
  } catch (error) {
    errorMessage.value = trRequestError(toRequestError(error))
  } finally {
    isLoading.value = false
  }
}

async function changePage(target: number) {
  if (target < FIRST_PAGE || target > totalPages.value || target === page.value) return

  page.value = target
  selectedId.value = null
  await loadSecrets()
}

function resetForm() {
  editingId.value = null
  formName.value = ''
  formUsername.value = ''
  formPassword.value = ''
}

function openCreateForm() {
  resetForm()
  isFormVisible.value = true
}

function openEditForm(item: SecretItem) {
  editingId.value = item.id
  formName.value = item.name
  formUsername.value = item.username
  formPassword.value = ''
  isFormVisible.value = true
}

function closeForm() {
  isFormVisible.value = false
  resetForm()
}

function selectRow(item: SecretItem) {
  selectedId.value = selectedId.value === item.id ? null : item.id
}

function editSelected() {
  if (selectedItem.value !== null) {
    openEditForm(selectedItem.value)
  }
}

function deleteSelected() {
  if (selectedItem.value !== null) {
    remove(selectedItem.value)
  }
}

async function save() {
  const name = formName.value.trim()
  const username = formUsername.value.trim()

  const validationError = validateSecretForm({
    name,
    username,
    password: formPassword.value,
    isEditing: editingId.value !== null
  })
  if (validationError) {
    errorMessage.value = validationError
    return
  }

  isSaving.value = true
  errorMessage.value = ''

  try {
    if (editingId.value === null) {
      await createSecret({
        name,
        username,
        password: formPassword.value
      })
    } else {
      await updateSecret(editingId.value, {
        name,
        username,
        password: formPassword.value.length === 0 ? null : formPassword.value
      })
    }

    closeForm()
    await loadSecrets()
  } catch (error) {
    errorMessage.value = trRequestError(toRequestError(error))
  } finally {
    isSaving.value = false
  }
}

async function remove(item: SecretItem) {
  if (isDeleting.value) return

  if (!window.confirm(tr(RESOURCE_KEYS.SECRETS_DELETE_CONFIRM))) return

  errorMessage.value = ''
  isDeleting.value = true

  try {
    await deleteSecret(item.id)
    selectedId.value = null
    await loadSecrets()
  } catch (error) {
    errorMessage.value = trRequestError(toRequestError(error))
  } finally {
    isDeleting.value = false
  }
}

function logout() {
  authStore.logout()
  router.push(ROUTES.LOGIN)
}

onMounted(loadSecrets)
</script>

<template>
  <div class="secrets">
    <header class="secrets__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <nav class="secrets__nav">
        <button class="secrets__nav-link" @click="router.push(ROUTES.HOME)">
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
          <button
            class="secrets__button"
            :disabled="selectedItem === null"
            @click="editSelected"
          >
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
          <button type="button" class="secrets__button secrets__button--secondary" @click="closeForm">
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

<style scoped>
.secrets {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.secrets__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-3);
  padding: var(--space-3) var(--space-5);
  background: var(--color-surface);
  border-bottom: var(--border-width) solid var(--color-border);
}

.secrets__header h1 {
  font-size: var(--font-size-heading);
}

.secrets__nav {
  display: flex;
  gap: var(--space-2);
}

.secrets__nav-link {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: none;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
  color: var(--color-text-secondary);
}

.secrets__nav-link:hover {
  background: var(--color-bg);
}

.secrets__nav-link--active {
  color: var(--color-text);
  font-weight: 600;
}

.secrets__logout {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.secrets__logout:hover {
  background: var(--color-bg);
}

.secrets__body {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  flex: 1;
  width: 100%;
  max-width: 720px;
  margin: 0 auto;
  padding: var(--space-5) var(--space-4);
}

.secrets__toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.secrets__toolbar h2 {
  font-size: var(--font-size-large);
}

.secrets__toolbar-actions {
  display: flex;
  gap: var(--space-2);
}

.secrets__add {
  padding: var(--space-2) var(--space-3);
  background: var(--color-primary);
  color: var(--color-on-primary);
  border: none;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.secrets__form {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  padding: var(--space-4);
  background: var(--color-surface);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius);
}

.secrets__field {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}

.secrets__field input {
  padding: var(--space-2) var(--space-3);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
  outline: none;
}

.secrets__field input:focus {
  border-color: var(--color-primary);
}

.secrets__form-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-2);
}

.secrets__button {
  padding: var(--space-2) var(--space-3);
  background: var(--color-primary);
  color: var(--color-on-primary);
  border: none;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.secrets__button:disabled {
  opacity: 0.5;
  cursor: default;
}

.secrets__button--secondary {
  background: transparent;
  color: var(--color-text-secondary);
  border: var(--border-width) solid var(--color-border);
}

.secrets__button--danger {
  background: transparent;
  color: var(--color-error);
  border: var(--border-width) solid var(--color-error);
}

.secrets__error {
  color: var(--color-error);
  font-size: var(--font-size-small);
}

.secrets__loading,
.secrets__empty {
  text-align: center;
  color: var(--color-text-muted);
  font-size: var(--font-size-base);
}

.secrets__table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius);
}

.secrets__table th,
.secrets__table td {
  padding: var(--space-3);
  text-align: left;
  border-bottom: var(--border-width) solid var(--color-border);
  font-size: var(--font-size-base);
}

.secrets__table th {
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}

.secrets__row {
  cursor: pointer;
}

.secrets__row:hover {
  background: var(--color-bg);
}

.secrets__row--selected,
.secrets__row--selected:hover {
  background: var(--color-primary-disabled);
}

.secrets__pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-3);
}

.secrets__pagination-info {
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}
</style>
