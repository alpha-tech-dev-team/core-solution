<script setup lang="ts">
import LogoutButton from '@shared/components/LogoutButton.vue'
import PaginationNav from '@shared/components/PaginationNav.vue'
import { LOADING_PLACEHOLDER } from '@shared/constants/ui'
import { useTranslations } from '@shared/i18n/create-i18n'
import { AUDIT_SORT_FIELDS } from '@/constants/audit'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { ROUTE_NAMES } from '@/constants/routes'
import { useAuditView } from './use-audit-view'

const { tr } = useTranslations()
const audit = useAuditView()

const {
  query,
  actionOptions,
  sourceOptions,
  entries,
  page,
  totalPages,
  isLoading,
  errorMessage,
  tooManyMatches,
  changePage,
  toggleSort,
  isSortedBy,
  sortAriaValue,
  sortIndicatorClass,
  onSearchInput,
  applyFilters,
  auditActionLabel,
  auditSourceLabel,
  actorName,
} = audit
</script>

<template>
  <div class="page">
    <header class="page__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <nav class="page__nav">
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.HOME }">
          {{ tr(RESOURCE_KEYS.NAV_HOME) }}
        </RouterLink>
      </nav>
      <LogoutButton />
    </header>

    <main class="page__body">
      <div class="toolbar">
        <h2>{{ tr(RESOURCE_KEYS.AUDIT_TITLE) }}</h2>
      </div>

      <div class="toolbar audit__filters">
        <input
          v-model="query.search"
          class="input audit__search"
          type="search"
          :placeholder="tr(RESOURCE_KEYS.AUDIT_SEARCH_PLACEHOLDER)"
          @input="onSearchInput"
        />
        <label class="field">
          <span class="field__label">{{ tr(RESOURCE_KEYS.AUDIT_COLUMN_ACTION) }}</span>
          <select v-model="query.action" class="input" @change="applyFilters">
            <option value="">{{ tr(RESOURCE_KEYS.AUDIT_FILTER_ALL) }}</option>
            <option v-for="action in actionOptions" :key="action" :value="action">
              {{ auditActionLabel(action) }}
            </option>
          </select>
        </label>
        <label class="field">
          <span class="field__label">{{ tr(RESOURCE_KEYS.AUDIT_COLUMN_SOURCE) }}</span>
          <select v-model="query.requestSource" class="input" @change="applyFilters">
            <option value="">{{ tr(RESOURCE_KEYS.AUDIT_FILTER_ALL) }}</option>
            <option v-for="source in sourceOptions" :key="source" :value="source">
              {{ auditSourceLabel(source) }}
            </option>
          </select>
        </label>
      </div>

      <p v-if="tooManyMatches" class="alert alert--error">
        {{ tr(RESOURCE_KEYS.AUDIT_TOO_MANY_MATCHES) }}
      </p>

      <p v-if="errorMessage" class="alert alert--error">{{ errorMessage }}</p>
      <p v-if="isLoading && !tooManyMatches" class="text-muted">{{ LOADING_PLACEHOLDER }}</p>

      <p v-else-if="entries.length === 0 && !tooManyMatches" class="text-muted">
        {{ tr(RESOURCE_KEYS.AUDIT_EMPTY) }}
      </p>

      <table v-else-if="!tooManyMatches" class="table">
        <thead>
          <tr>
            <th :aria-sort="sortAriaValue(AUDIT_SORT_FIELDS.TIMESTAMP)">
              <button
                type="button"
                class="audit__sort"
                :class="{ 'audit__sort--active': isSortedBy(AUDIT_SORT_FIELDS.TIMESTAMP) }"
                @click="toggleSort(AUDIT_SORT_FIELDS.TIMESTAMP)"
              >
                {{ tr(RESOURCE_KEYS.AUDIT_COLUMN_TIME) }}
                <span
                  class="audit__sort-indicator"
                  :class="sortIndicatorClass(AUDIT_SORT_FIELDS.TIMESTAMP)"
                  aria-hidden="true"
                />
              </button>
            </th>
            <th>{{ tr(RESOURCE_KEYS.AUDIT_COLUMN_USER) }}</th>
            <th :aria-sort="sortAriaValue(AUDIT_SORT_FIELDS.ACTION)">
              <button
                type="button"
                class="audit__sort"
                :class="{ 'audit__sort--active': isSortedBy(AUDIT_SORT_FIELDS.ACTION) }"
                @click="toggleSort(AUDIT_SORT_FIELDS.ACTION)"
              >
                {{ tr(RESOURCE_KEYS.AUDIT_COLUMN_ACTION) }}
                <span
                  class="audit__sort-indicator"
                  :class="sortIndicatorClass(AUDIT_SORT_FIELDS.ACTION)"
                  aria-hidden="true"
                />
              </button>
            </th>
            <th :aria-sort="sortAriaValue(AUDIT_SORT_FIELDS.TARGET_ITEM_ID)">
              <button
                type="button"
                class="audit__sort"
                :class="{ 'audit__sort--active': isSortedBy(AUDIT_SORT_FIELDS.TARGET_ITEM_ID) }"
                @click="toggleSort(AUDIT_SORT_FIELDS.TARGET_ITEM_ID)"
              >
                {{ tr(RESOURCE_KEYS.AUDIT_COLUMN_TARGET) }}
                <span
                  class="audit__sort-indicator"
                  :class="sortIndicatorClass(AUDIT_SORT_FIELDS.TARGET_ITEM_ID)"
                  aria-hidden="true"
                />
              </button>
            </th>
            <th :aria-sort="sortAriaValue(AUDIT_SORT_FIELDS.REQUEST_SOURCE)">
              <button
                type="button"
                class="audit__sort"
                :class="{ 'audit__sort--active': isSortedBy(AUDIT_SORT_FIELDS.REQUEST_SOURCE) }"
                @click="toggleSort(AUDIT_SORT_FIELDS.REQUEST_SOURCE)"
              >
                {{ tr(RESOURCE_KEYS.AUDIT_COLUMN_SOURCE) }}
                <span
                  class="audit__sort-indicator"
                  :class="sortIndicatorClass(AUDIT_SORT_FIELDS.REQUEST_SOURCE)"
                  aria-hidden="true"
                />
              </button>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="entry in entries" :key="entry.id">
            <td class="numeric">{{ new Date(entry.timestamp).toLocaleString() }}</td>
            <td>{{ actorName(entry) }}</td>
            <td>{{ auditActionLabel(entry.action) }}</td>
            <td>{{ entry.targetItemId ?? '' }}</td>
            <td>{{ auditSourceLabel(entry.requestSource) }}</td>
          </tr>
        </tbody>
      </table>

      <PaginationNav
        v-if="!tooManyMatches"
        :page="page"
        :total-pages="totalPages"
        :prev-label="tr(RESOURCE_KEYS.AUDIT_PAGE_PREV)"
        :next-label="tr(RESOURCE_KEYS.AUDIT_PAGE_NEXT)"
        :info-label="tr(RESOURCE_KEYS.AUDIT_PAGE_INFO, page, totalPages)"
        @change="changePage($event)"
      />
    </main>
  </div>
</template>

<style scoped>
.audit__filters {
  flex-wrap: wrap;
  align-items: flex-end;
  margin-bottom: var(--space-4);
}

.audit__search {
  flex: 1 1 240px;
}

.audit__sort {
  display: inline-flex;
  align-items: center;
  gap: var(--space-1);
  padding: 0;
  background: transparent;
  border: none;
  color: inherit;
  font: inherit;
  cursor: pointer;
}

.audit__sort-indicator {
  width: 0;
  height: 0;
  border-right: 4px solid transparent;
  border-left: 4px solid transparent;
  opacity: 0.35;
}

.audit__sort-indicator--ascending {
  border-bottom: 5px solid currentColor;
  opacity: 1;
}

.audit__sort-indicator--descending {
  border-top: 5px solid currentColor;
  opacity: 1;
}

.audit__sort--active {
  color: var(--color-text);
}
</style>
