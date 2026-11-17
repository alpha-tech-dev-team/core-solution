<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { tr, trAuditAction, trAuditSource, trRequestError } from '@/i18n'
import { AUDIT_ACTIONS, AUDIT_SOURCES, RESOURCE_KEYS } from '@/constants/i18n'
import { ROUTES } from '@/constants/routes'
import { AUDIT_PAGE_SIZE, FIRST_PAGE } from '@/constants/pagination'
import { LOADING_PLACEHOLDER } from '@/constants/ui'
import {
  AUDIT_DEFAULT_SORT_BY,
  AUDIT_DEFAULT_SORT_DIRECTION,
  AUDIT_SEARCH_DEBOUNCE_MS,
  AUDIT_SORT_FIELDS
} from '@/constants/audit'
import { toRequestError } from '@/api/request-error'
import { fetchAuditEntries } from '@/api/audit'
import { fetchEmployeeNames, type EmployeeNames } from '@/api/employees'
import { createAuditHubConnection } from '@/api/audit-hub'
import type { AuditEntry, AuditQuery } from '@/stores/models/audit-entry'

const router = useRouter()
const authStore = useAuthStore()

const entries = ref<AuditEntry[]>([])
const employeeNames = ref<EmployeeNames>({ byId: new Map(), byTelegramId: new Map() })
const areEmployeeNamesLoading = ref(false)
const page = ref(FIRST_PAGE)
const total = ref(0)
const isLoading = ref(false)
const errorMessage = ref('')

const query = reactive<AuditQuery>({
  search: '',
  action: '',
  requestSource: '',
  sortBy: AUDIT_DEFAULT_SORT_BY,
  sortDirection: AUDIT_DEFAULT_SORT_DIRECTION
})

const actionOptions = Object.keys(AUDIT_ACTIONS)
const sourceOptions = Object.keys(AUDIT_SOURCES)

const isDefaultView = computed(() =>
  !query.search
  && !query.action
  && !query.requestSource
  && query.sortBy === AUDIT_DEFAULT_SORT_BY
  && query.sortDirection === AUDIT_DEFAULT_SORT_DIRECTION
)

const totalPages = computed(() => Math.max(FIRST_PAGE, Math.ceil(total.value / AUDIT_PAGE_SIZE)))
const canGoBack = computed(() => page.value > FIRST_PAGE)
const canGoForward = computed(() => page.value < totalPages.value)

async function loadEntries() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const result = await fetchAuditEntries(page.value, AUDIT_PAGE_SIZE, query)
    entries.value = result.items
    total.value = result.total
  } catch (error) {
    errorMessage.value = trRequestError(toRequestError(error))
  } finally {
    isLoading.value = false
  }
}

async function applyQuery() {
  page.value = FIRST_PAGE
  await loadEntries()
}

let searchTimer: number | undefined

function onSearchInput() {
  window.clearTimeout(searchTimer)
  searchTimer = window.setTimeout(() => void applyQuery(), AUDIT_SEARCH_DEBOUNCE_MS)
}

function toggleSort(field: string) {
  if (query.sortBy === field) {
    query.sortDirection = query.sortDirection === 'asc' ? 'desc' : 'asc'
  } else {
    query.sortBy = field
    query.sortDirection = 'asc'
  }

  void applyQuery()
}

function sortIndicator(field: string): string {
  if (query.sortBy !== field) return ''

  return query.sortDirection === 'asc' ? '▲' : '▼'
}

function isSortedBy(field: string): boolean {
  return query.sortBy === field
}

function actorName(entry: AuditEntry): string {
  if (entry.telegramUserId !== null) {
    return employeeNames.value.byTelegramId.get(entry.telegramUserId) ?? String(entry.telegramUserId)
  }

  if (entry.userId !== null) {
    return employeeNames.value.byId.get(entry.userId) ?? entry.userId
  }

  return ''
}

function knowsActor(entry: AuditEntry): boolean {
  if (entry.telegramUserId !== null) {
    return employeeNames.value.byTelegramId.has(entry.telegramUserId)
  }

  if (entry.userId !== null) {
    return employeeNames.value.byId.has(entry.userId)
  }

  return true
}

async function loadEmployeeNames() {
  if (areEmployeeNamesLoading.value) return

  areEmployeeNamesLoading.value = true

  try {
    employeeNames.value = await fetchEmployeeNames()
  } catch (error) {
    console.error('Не удалось загрузить сотрудников для журнала действий', error)
  } finally {
    areEmployeeNamesLoading.value = false
  }
}

async function changePage(target: number) {
  if (target < FIRST_PAGE || target > totalPages.value || target === page.value) return

  page.value = target
  await loadEntries()
}

async function logout() {
  await authStore.logout()
  router.push(ROUTES.LOGIN)
}

const auditHub = createAuditHubConnection(addLiveEntry)

function addLiveEntry(entry: AuditEntry) {
  if (entries.value.some((existing) => existing.id === entry.id)) return

  if (isDefaultView.value) {
    total.value += 1

    if (page.value === FIRST_PAGE) {
      entries.value = [entry, ...entries.value]
    }
  } else {
    void loadEntries()
  }

  if (!knowsActor(entry)) {
    void loadEmployeeNames()
  }
}

async function startLiveUpdates() {
  try {
    await auditHub.start()
  } catch (error) {
    console.error('Не удалось подключиться к каналу аудита', error)
  }
}

onMounted(() => {
  void loadEntries()
  void loadEmployeeNames()
  void startLiveUpdates()
})

onUnmounted(() => {
  window.clearTimeout(searchTimer)
  void auditHub.stop()
})
</script>

<template>
  <div class="audit">
    <header class="audit__header">
      <h1>{{ tr(RESOURCE_KEYS.AUDIT_TITLE) }}</h1>
      <nav class="audit__nav">
        <button @click="router.push(ROUTES.HOME)">{{ tr(RESOURCE_KEYS.APP_TITLE) }}</button>
        <button @click="logout">{{ tr(RESOURCE_KEYS.AUTH_LOGOUT) }}</button>
      </nav>
    </header>

    <main class="audit__body">
      <div class="audit__toolbar">
        <input
          v-model="query.search"
          type="search"
          class="audit__search"
          :placeholder="tr(RESOURCE_KEYS.AUDIT_SEARCH_PLACEHOLDER)"
          @input="onSearchInput"
        />
        <label class="audit__filter">
          <span>{{ tr(RESOURCE_KEYS.AUDIT_COLUMN_ACTION) }}</span>
          <select v-model="query.action" @change="applyQuery">
            <option value="">{{ tr(RESOURCE_KEYS.AUDIT_FILTER_ALL) }}</option>
            <option v-for="action in actionOptions" :key="action" :value="action">
              {{ trAuditAction(action) }}
            </option>
          </select>
        </label>
        <label class="audit__filter">
          <span>{{ tr(RESOURCE_KEYS.AUDIT_COLUMN_SOURCE) }}</span>
          <select v-model="query.requestSource" @change="applyQuery">
            <option value="">{{ tr(RESOURCE_KEYS.AUDIT_FILTER_ALL) }}</option>
            <option v-for="source in sourceOptions" :key="source" :value="source">
              {{ trAuditSource(source) }}
            </option>
          </select>
        </label>
      </div>

      <p v-if="errorMessage" class="audit__error">{{ errorMessage }}</p>
      <p v-else-if="isLoading" class="audit__placeholder">{{ LOADING_PLACEHOLDER }}</p>
      <p v-else-if="entries.length === 0" class="audit__placeholder">{{ tr(RESOURCE_KEYS.AUDIT_EMPTY) }}</p>

      <table v-else class="audit__table">
        <thead>
          <tr>
            <th>
              <button
                type="button"
                class="audit__sort"
                :class="{ 'audit__sort--active': isSortedBy(AUDIT_SORT_FIELDS.TIMESTAMP) }"
                @click="toggleSort(AUDIT_SORT_FIELDS.TIMESTAMP)"
              >
                {{ tr(RESOURCE_KEYS.AUDIT_COLUMN_TIME) }} {{ sortIndicator(AUDIT_SORT_FIELDS.TIMESTAMP) }}
              </button>
            </th>
            <th>{{ tr(RESOURCE_KEYS.AUDIT_COLUMN_USER) }}</th>
            <th>
              <button
                type="button"
                class="audit__sort"
                :class="{ 'audit__sort--active': isSortedBy(AUDIT_SORT_FIELDS.ACTION) }"
                @click="toggleSort(AUDIT_SORT_FIELDS.ACTION)"
              >
                {{ tr(RESOURCE_KEYS.AUDIT_COLUMN_ACTION) }} {{ sortIndicator(AUDIT_SORT_FIELDS.ACTION) }}
              </button>
            </th>
            <th>
              <button
                type="button"
                class="audit__sort"
                :class="{ 'audit__sort--active': isSortedBy(AUDIT_SORT_FIELDS.TARGET_ITEM_ID) }"
                @click="toggleSort(AUDIT_SORT_FIELDS.TARGET_ITEM_ID)"
              >
                {{ tr(RESOURCE_KEYS.AUDIT_COLUMN_TARGET) }} {{ sortIndicator(AUDIT_SORT_FIELDS.TARGET_ITEM_ID) }}
              </button>
            </th>
            <th>
              <button
                type="button"
                class="audit__sort"
                :class="{ 'audit__sort--active': isSortedBy(AUDIT_SORT_FIELDS.REQUEST_SOURCE) }"
                @click="toggleSort(AUDIT_SORT_FIELDS.REQUEST_SOURCE)"
              >
                {{ tr(RESOURCE_KEYS.AUDIT_COLUMN_SOURCE) }} {{ sortIndicator(AUDIT_SORT_FIELDS.REQUEST_SOURCE) }}
              </button>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="entry in entries" :key="entry.id">
            <td>{{ new Date(entry.timestamp).toLocaleString() }}</td>
            <td>{{ actorName(entry) }}</td>
            <td>{{ trAuditAction(entry.action) }}</td>
            <td>{{ entry.targetItemId ?? '' }}</td>
            <td>{{ trAuditSource(entry.requestSource) }}</td>
          </tr>
        </tbody>
      </table>

      <footer v-if="!isLoading && entries.length > 0" class="audit__pager">
        <button :disabled="!canGoBack" @click="changePage(page - 1)">
          {{ tr(RESOURCE_KEYS.AUDIT_PREV_PAGE) }}
        </button>
        <span>{{ tr(RESOURCE_KEYS.AUDIT_PAGE_INFO, page, totalPages, total) }}</span>
        <button :disabled="!canGoForward" @click="changePage(page + 1)">
          {{ tr(RESOURCE_KEYS.AUDIT_NEXT_PAGE) }}
        </button>
      </footer>
    </main>
  </div>
</template>

<style scoped>
.audit {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.audit__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-3) var(--space-5);
  background: var(--color-surface);
  border-bottom: var(--border-width) solid var(--color-border);
}

.audit__header h1 {
  font-size: var(--font-size-heading);
}

.audit__nav {
  display: flex;
  gap: var(--space-2);
}

.audit__nav button,
.audit__pager button {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.audit__nav button:hover,
.audit__pager button:hover:not(:disabled) {
  background: var(--color-bg);
}

.audit__pager button:disabled {
  color: var(--color-text-muted);
  cursor: default;
}

.audit__body {
  flex: 1;
  padding: var(--space-5);
  overflow-x: auto;
}

.audit__toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  gap: var(--space-3);
  margin-bottom: var(--space-4);
}

.audit__search {
  flex: 1 1 240px;
  padding: var(--space-2) var(--space-3);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-small);
}

.audit__filter {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}

.audit__filter select {
  padding: var(--space-2) var(--space-3);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-small);
}

.audit__sort {
  padding: 0;
  background: transparent;
  border: none;
  color: inherit;
  font-size: inherit;
  cursor: pointer;
}

.audit__sort--active {
  color: var(--color-text);
}

.audit__table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius);
}

.audit__table th,
.audit__table td {
  padding: var(--space-2) var(--space-3);
  text-align: left;
  border-bottom: var(--border-width) solid var(--color-border);
  font-size: var(--font-size-small);
}

.audit__table th {
  color: var(--color-text-secondary);
  font-weight: normal;
}

.audit__pager {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-3);
  margin-top: var(--space-4);
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}

.audit__placeholder {
  text-align: center;
  color: var(--color-text-muted);
}

.audit__error {
  text-align: center;
  color: var(--color-error);
}
</style>
