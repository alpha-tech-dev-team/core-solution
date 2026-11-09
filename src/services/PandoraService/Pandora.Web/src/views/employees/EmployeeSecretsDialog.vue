<script setup lang="ts">
import { useTranslations } from '@shared/i18n/create-i18n'
import { LOADING_PLACEHOLDER } from '@shared/constants/ui'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { formatEmployeeFullName } from '@/models/employee'
import type { Employee } from '@/models/employee'
import type { EmployeeSecret } from '@/models/employee-secret'
import ModalDialog from '@shared/components/ModalDialog.vue'
import PaginationNav from '@shared/components/PaginationNav.vue'

const props = defineProps<{
  open: boolean
  employee: Employee | null
  secrets: EmployeeSecret[]
  isLoading: boolean
  savingItemId: string | null
  error: string
  page: number
  totalPages: number
}>()

const emit = defineEmits<{
  toggle: [item: EmployeeSecret]
  pageChange: [page: number]
  close: []
}>()

const { tr } = useTranslations()

function hasTelegram(): boolean {
  return props.employee !== null && props.employee.telegramUserId !== null
}
</script>

<template>
  <ModalDialog
    :open="open"
    :title="
      tr(
        RESOURCE_KEYS.EMPLOYEE_SECRETS_TITLE,
        employee === null ? '' : formatEmployeeFullName(employee),
      )
    "
    @close="emit('close')"
  >
    <p v-if="employee !== null && !hasTelegram()" class="text-muted">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_SECRETS_NO_TELEGRAM) }}
    </p>

    <p v-else-if="isLoading" class="text-muted">{{ LOADING_PLACEHOLDER }}</p>

    <p v-else-if="secrets.length === 0" class="text-muted">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_SECRETS_EMPTY) }}
    </p>

    <template v-else>
      <p class="text-muted text-sm">{{ tr(RESOURCE_KEYS.EMPLOYEE_SECRETS_HINT) }}</p>
      <ul class="secrets-list">
        <li v-for="item in secrets" :key="item.id">
          <label class="checkbox secrets-list__row">
            <input
              type="checkbox"
              :checked="item.isGranted"
              :disabled="savingItemId !== null"
              @change="emit('toggle', item)"
            />
            <span class="secrets-list__name">{{ item.name }}</span>
            <span class="secrets-list__username">{{ item.username }}</span>
          </label>
        </li>
      </ul>
    </template>

    <PaginationNav
      :page="page"
      :total-pages="totalPages"
      :prev-label="tr(RESOURCE_KEYS.SECRETS_PAGE_PREV)"
      :next-label="tr(RESOURCE_KEYS.SECRETS_PAGE_NEXT)"
      :info-label="tr(RESOURCE_KEYS.SECRETS_PAGE_INFO, page, totalPages)"
      @change="emit('pageChange', $event)"
    />

    <p v-if="error" class="field__error">{{ error }}</p>

    <div class="form__actions">
      <button type="button" class="btn btn--ghost" @click="emit('close')">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_SECRETS_CLOSE) }}
      </button>
    </div>
  </ModalDialog>
</template>

<style scoped>
.secrets-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  max-height: 50vh;
  margin: 0;
  padding: 0;
  overflow-y: auto;
  list-style: none;
}

.secrets-list__row {
  padding: var(--space-2);
  border-radius: var(--radius-sm);
  cursor: pointer;
}

.secrets-list__row:hover {
  background: var(--color-surface-muted);
}

.secrets-list__name {
  flex: 1;
}

.secrets-list__username {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}
</style>
