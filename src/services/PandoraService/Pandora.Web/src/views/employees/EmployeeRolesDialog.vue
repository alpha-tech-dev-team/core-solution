<script setup lang="ts">
import { tr } from '@/i18n'
import type { Employee } from '@/stores/models/employee'
import ModalDialog from '@/components/ModalDialog.vue'

defineProps<{
  open: boolean
  employee: Employee | null
  roles: string[]
  error: string
}>()

const emit = defineEmits<{
  toggle: [payload: { role: string; checked: boolean }]
  close: []
}>()

function hasRole(employee: Employee, role: string): boolean {
  return employee.roles.includes(role)
}
</script>

<template>
  <ModalDialog :open="open" :title="tr('employee_roles_title')" @close="emit('close')">
    <p class="dialog__subtitle">{{ employee ? [employee.surname, employee.name].filter(Boolean).join(' ') : '' }}</p>

    <div v-if="employee" class="dialog__roles">
      <label v-for="role in roles" :key="role" class="dialog__role">
        <input
          type="checkbox"
          :checked="hasRole(employee, role)"
          @change="emit('toggle', { role, checked: ($event.target as HTMLInputElement).checked })"
        />
        {{ role }}
      </label>
    </div>

    <p v-if="error" class="dialog__error">{{ error }}</p>

    <div class="dialog__actions">
      <button type="button" class="dialog__button" @click="emit('close')">{{ tr('employee_close') }}</button>
    </div>
  </ModalDialog>
</template>

<style scoped>
.dialog__subtitle {
  font-size: var(--font-size-base);
  color: var(--color-text-secondary);
}

.dialog__roles {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.dialog__role {
  display: flex;
  align-items: center;
  gap: var(--space-2);
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
</style>
