<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { isAdminRole, PANDORA_ADMIN_ROLE } from '@/constants/roles'
import type { Employee } from '@/models/employee'
import type { EmployeeRolesPayload } from '@/models/employee-roles-payload'
import ModalDialog from '@/components/ModalDialog.vue'

const props = defineProps<{
  open: boolean
  employee: Employee | null
  roles: string[]
  isSelf: boolean
  error: string
}>()

const emit = defineEmits<{
  save: [payload: EmployeeRolesPayload]
  close: []
}>()

const { tr } = useTranslations()

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
  },
)

const needsCredentials = computed(() => selectedRoles.value.some(isAdminRole))

const losesOwnAccess = computed(
  () => props.isSelf && !selectedRoles.value.includes(PANDORA_ADMIN_ROLE),
)

const canSave = computed(
  () => !needsCredentials.value || (adminLogin.value.trim() !== '' && adminPassword.value !== ''),
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
    adminPassword: adminPassword.value || null,
  })
}
</script>

<template>
  <ModalDialog :open="open" :title="tr(RESOURCE_KEYS.EMPLOYEE_ROLES_TITLE)" @close="emit('close')">
    <p v-if="employee" class="text-muted">
      {{ [employee.surname, employee.name].filter(Boolean).join(' ') }}
    </p>

    <div v-if="employee" class="form">
      <label v-for="role in roles" :key="role" class="checkbox">
        <input
          type="checkbox"
          :checked="selectedRoles.includes(role)"
          @change="toggle(role, ($event.target as HTMLInputElement).checked)"
        />
        {{ role }}
      </label>
    </div>

    <label v-if="needsCredentials" class="field">
      <span class="field__label">{{ tr(RESOURCE_KEYS.EMPLOYEE_ADMIN_LOGIN) }}</span>
      <input v-model="adminLogin" class="input" type="text" />
    </label>
    <label v-if="needsCredentials" class="field">
      <span class="field__label">{{ tr(RESOURCE_KEYS.EMPLOYEE_ADMIN_PASSWORD) }}</span>
      <input v-model="adminPassword" class="input" type="password" />
    </label>

    <p v-if="losesOwnAccess" class="alert alert--error">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_ROLES_SELF_WARNING) }}
    </p>

    <p v-if="error" class="field__error">{{ error }}</p>

    <div class="form__actions">
      <button type="button" class="btn btn--primary" :disabled="!canSave" @click="save">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_SAVE) }}
      </button>
      <button type="button" class="btn btn--ghost" @click="emit('close')">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_CLOSE) }}
      </button>
    </div>
  </ModalDialog>
</template>
