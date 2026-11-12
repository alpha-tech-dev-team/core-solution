<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { tr } from '@/i18n'
import type { Employee } from '@/stores/models/employee'
import { isAdminRole, PANDORA_ADMIN_ROLE } from '@/constants/roles'
import ModalDialog from '@/components/ModalDialog.vue'

const props = defineProps<{
  open: boolean
  employee: Employee | null
  roles: string[]
  isSelf: boolean
  error: string
}>()

const emit = defineEmits<{
  save: [payload: { roles: string[]; adminLogin: string | null; adminPassword: string | null }]
  close: []
}>()

const selectedRoles = ref<string[]>([])
const adminLogin = ref('')
const adminPassword = ref('')

watch(
  () => props.open,
  (open) => {
    if (!open) return

    selectedRoles.value = [...(props.employee?.roles ?? [])]
    adminLogin.value = props.employee?.adminUsername ?? ''
    adminPassword.value = ''
  }
)

const needsCredentials = computed(() => selectedRoles.value.some(isAdminRole))

const losesOwnAccess = computed(() => props.isSelf && !selectedRoles.value.includes(PANDORA_ADMIN_ROLE))

const canSave = computed(
  () => !needsCredentials.value || (adminLogin.value.trim() !== '' && adminPassword.value !== '')
)

function toggle(role: string, checked: boolean): void {
  selectedRoles.value = checked
    ? [...selectedRoles.value, role]
    : selectedRoles.value.filter((item) => item !== role)
}

function save(): void {
  emit('save', {
    roles: selectedRoles.value,
    adminLogin: adminLogin.value.trim() || null,
    adminPassword: adminPassword.value || null
  })
}
</script>

<template>
  <ModalDialog :open="open" :title="tr('employee_roles_title')" @close="emit('close')">
    <p class="dialog__subtitle">{{ employee ? [employee.surname, employee.name].filter(Boolean).join(' ') : '' }}</p>

    <div v-if="employee" class="dialog__roles">
      <label v-for="role in roles" :key="role" class="dialog__role">
        <input
          type="checkbox"
          :checked="selectedRoles.includes(role)"
          @change="toggle(role, ($event.target as HTMLInputElement).checked)"
        />
        {{ role }}
      </label>
    </div>

    <label v-if="needsCredentials" class="dialog__field">
      {{ tr('employee_admin_login') }}
      <input v-model="adminLogin" type="text" />
    </label>
    <label v-if="needsCredentials" class="dialog__field">
      {{ tr('employee_admin_password') }}
      <input v-model="adminPassword" type="password" />
    </label>

    <p v-if="losesOwnAccess" class="dialog__warning">{{ tr('employee_roles_self_warning') }}</p>

    <p v-if="error" class="dialog__error">{{ error }}</p>

    <div class="dialog__actions">
      <button type="button" class="dialog__button" :disabled="!canSave" @click="save">{{ tr('employee_save') }}</button>
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

.dialog__field {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  font-size: var(--font-size-small);
}

.dialog__field input {
  padding: var(--space-2);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.dialog__warning {
  color: var(--color-error);
  font-size: var(--font-size-small);
  font-weight: 600;
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
