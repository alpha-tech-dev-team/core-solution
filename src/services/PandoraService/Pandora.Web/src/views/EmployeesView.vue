<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useEmployeesStore } from '@/stores/users'
import { EMPLOYEE_STATUS } from '@/stores/models/employee-status'
import type { Employee } from '@/stores/models/employee'
import { tr, trRequestError } from '@/i18n'
import { EMPLOYEE_STATUS_RESOURCE_KEYS, RESOURCE_KEYS } from '@/constants/i18n'
import { ROUTES } from '@/constants/routes'
import { LOADING_PLACEHOLDER } from '@/constants/ui'

const router = useRouter()
const authStore = useAuthStore()
const employeesStore = useEmployeesStore()
const errorMessage = ref('')

async function logout() {
  await authStore.logout()
  router.push(ROUTES.LOGIN)
}

async function loadEmployees() {
  errorMessage.value = ''
  const error = await employeesStore.load()
  if (error !== null) errorMessage.value = trRequestError(error)
}

function fullName(employee: Employee): string {
  return [employee.surname, employee.name, employee.patronymic].filter(Boolean).join(' ')
}

function statusLabel(status: string): string {
  return tr(EMPLOYEE_STATUS_RESOURCE_KEYS[status] ?? status)
}

function hasRole(employee: Employee, role: string): boolean {
  return employee.roles.includes(role)
}

async function toggleRole(employee: Employee, role: string, checked: boolean) {
  const roles = checked
    ? [...employee.roles, role]
    : employee.roles.filter((item) => item !== role)

  errorMessage.value = ''
  const error = await employeesStore.assignRoles(employee, roles)
  if (error === null) return

  errorMessage.value = trRequestError(error)
  await employeesStore.load()
}

async function toggleBlock(employee: Employee) {
  const nextStatus = employee.status === EMPLOYEE_STATUS.BLOCKED
    ? EMPLOYEE_STATUS.ACTIVE
    : EMPLOYEE_STATUS.BLOCKED

  errorMessage.value = ''
  const error = await employeesStore.setStatus(employee, nextStatus)
  if (error === null) return

  errorMessage.value = trRequestError(error)
  await employeesStore.load()
}

onMounted(loadEmployees)
</script>

<template>
  <div class="employees-page">
    <header class="employees-page__header">
      <h1>{{ tr(RESOURCE_KEYS.EMPLOYEES_TITLE) }}</h1>
      <button class="employees-page__logout" @click="logout">{{ tr(RESOURCE_KEYS.AUTH_LOGOUT) }}</button>
    </header>

    <main class="employees-page__body">
      <p v-if="errorMessage" class="employees-page__error">{{ errorMessage }}</p>
      <p v-if="employeesStore.isLoading">{{ LOADING_PLACEHOLDER }}</p>
      <p v-else-if="employeesStore.employees.length === 0">{{ tr(RESOURCE_KEYS.EMPLOYEES_EMPTY) }}</p>

      <table v-else class="employees">
        <thead>
          <tr>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_NAME) }}</th>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_POSITION) }}</th>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_PHONE) }}</th>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_STATUS) }}</th>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_ROLES) }}</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="employee in employeesStore.employees" :key="employee.id">
            <td>{{ fullName(employee) }}</td>
            <td>{{ employee.position }}</td>
            <td>{{ employee.phone }}</td>
            <td>{{ statusLabel(employee.status) }}</td>
            <td class="employees__roles">
              <label v-for="role in employeesStore.roles" :key="role" class="employees__role">
                <input
                  type="checkbox"
                  :checked="hasRole(employee, role)"
                  @change="toggleRole(employee, role, ($event.target as HTMLInputElement).checked)"
                />
                {{ role }}
              </label>
            </td>
            <td>
              <button class="employees__action" @click="toggleBlock(employee)">
                {{ employee.status === EMPLOYEE_STATUS.BLOCKED
                  ? tr(RESOURCE_KEYS.EMPLOYEE_UNBLOCK)
                  : tr(RESOURCE_KEYS.EMPLOYEE_BLOCK) }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </main>
  </div>
</template>

<style scoped>
.employees-page {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.employees-page__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-3) var(--space-5);
  background: var(--color-surface);
  border-bottom: var(--border-width) solid var(--color-border);
}

.employees-page__header h1 {
  font-size: var(--font-size-heading);
}

.employees-page__logout {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.employees-page__logout:hover {
  background: var(--color-bg);
}

.employees-page__body {
  flex: 1;
  padding: var(--space-5);
  font-size: var(--font-size-base);
}

.employees-page__error {
  color: var(--color-error);
  font-size: var(--font-size-small);
  margin-bottom: var(--space-3);
}

.employees {
  width: 100%;
  background: var(--color-surface);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius);
  border-collapse: collapse;
}

.employees th,
.employees td {
  padding: var(--space-2) var(--space-3);
  text-align: left;
  border-bottom: var(--border-width) solid var(--color-border);
  vertical-align: top;
}

.employees th {
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}

.employees__roles {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.employees__role {
  display: flex;
  align-items: center;
  gap: var(--space-1);
  font-size: var(--font-size-small);
}

.employees__action {
  padding: var(--space-1) var(--space-2);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-small);
}

.employees__action:hover {
  background: var(--color-bg);
}
</style>
