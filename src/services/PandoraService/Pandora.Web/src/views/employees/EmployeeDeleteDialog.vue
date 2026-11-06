<script setup lang="ts">
import { tr } from '@/i18n'
import type { Employee } from '@/stores/models/employee'
import ModalDialog from '@/components/ModalDialog.vue'

defineProps<{
  open: boolean
  employee: Employee | null
  error: string
}>()

const emit = defineEmits<{
  confirm: []
  close: []
}>()

function fullName(employee: Employee): string {
  return [employee.surname, employee.name, employee.patronymic].filter(Boolean).join(' ')
}
</script>

<template>
  <ModalDialog :open="open" :title="tr('employee_delete_title')" @close="emit('close')">
    <p v-if="employee" class="dialog__question">
      {{ tr('employee_delete_question', fullName(employee)) }}
    </p>

    <p v-if="error" class="dialog__error">{{ error }}</p>

    <div class="dialog__actions">
      <button type="button" class="dialog__button dialog__button--danger" @click="emit('confirm')">
        {{ tr('employee_delete') }}
      </button>
      <button type="button" class="dialog__button" @click="emit('close')">
        {{ tr('employee_cancel') }}
      </button>
    </div>
  </ModalDialog>
</template>

<style scoped>
.dialog__question {
  font-size: var(--font-size-base);
}

.dialog__error {
  color: var(--color-error);
  font-size: var(--font-size-small);
}

.dialog__actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-2);
}

.dialog__button {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.dialog__button:hover {
  background: var(--color-bg);
}

.dialog__button--danger {
  color: var(--color-error);
  border-color: var(--color-error);
}
</style>
