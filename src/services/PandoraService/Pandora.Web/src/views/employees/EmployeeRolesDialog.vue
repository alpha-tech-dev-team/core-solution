<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { RESOURCE_KEYS } from '@/constants/i18n'
import { isAdminRole, PANDORA_ADMIN_ROLE } from '@/constants/roles'
import { tr } from '@/i18n'
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
    <p class="dialog__subtitle">
      {{ employee ? [employee.surname, employee.name].filter(Boolean).join(' ') : '' }}
    </p>

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
      {{ tr(RESOURCE_KEYS.EMPLOYEE_ADMIN_LOGIN) }}
      <input v-model="adminLogin" type="text" />
    </label>
    <label v-if="needsCredentials" class="dialog__field">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_ADMIN_PASSWORD) }}
      <input v-model="adminPassword" type="password" />
    </label>

    <p v-if="losesOwnAccess" class="dialog__warning">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_ROLES_SELF_WARNING) }}
    </p>

    <p v-if="error" class="dialog__error">{{ error }}</p>

    <div class="dialog__actions">
      <button type="button" class="dialog__button" :disabled="!canSave" @click="save">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_SAVE) }}
      </button>
      <button type="button" class="dialog__button" @click="emit('close')">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_CLOSE) }}
      </button>
    </div>
  </ModalDialog>
</template>

<style scoped src="./employee-roles-dialog.css"></style>
