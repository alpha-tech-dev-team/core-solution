<script setup lang="ts">
import { EMPLOYEE_STATUS } from '@/stores/models/employee-status'
import { tr, trStatus } from '@/i18n'
import { ROUTES } from '@/constants/routes'
import { LOADING_PLACEHOLDER } from '@/constants/ui'
import { useEmployeesPage } from './use-employees-page'
import EmployeeFormDialog from './EmployeeFormDialog.vue'
import EmployeeRolesDialog from './EmployeeRolesDialog.vue'
import EmployeeDeleteDialog from './EmployeeDeleteDialog.vue'

const page = useEmployeesPage()

const {
  employeesStore,
  errorMessage,
  selectedEmployee,
  selectedEmployeeId,
  isFormOpen,
  isRolesOpen,
  isSaving,
  isDeleteOpen,
  formInitial,
  fullName,
  logout
} = page
</script>

<template>
  <div class="employees-page">
    <header class="employees-page__header">
      <h1>{{ tr('employees_title') }}</h1>
      <nav class="employees-page__nav">
        <RouterLink class="employees-page__nav-link" :to="ROUTES.HOME">{{ tr('nav_home') }}</RouterLink>
        <button class="employees-page__nav-link" @click="logout">{{ tr('auth_logout') }}</button>
      </nav>
    </header>

    <main class="employees-page__body">
      <p v-if="errorMessage && !isFormOpen && !isRolesOpen" class="employees-page__error">
        {{ errorMessage }}
      </p>

      <div class="employees-toolbar">
        <div class="employees-toolbar__group">
          <button
            class="employees-page__nav-link"
            :disabled="selectedEmployee === null"
            @click="page.openRoles()"
          >
            {{ tr('employee_roles') }}
          </button>
          <button
            class="employees-page__nav-link"
            :disabled="selectedEmployee === null"
            @click="page.openEditForm()"
          >
            {{ tr('employee_edit') }}
          </button>
          <button
            class="employees-page__nav-link"
            :disabled="selectedEmployee === null"
            @click="page.toggleBlock()"
          >
            {{ selectedEmployee?.status === EMPLOYEE_STATUS.BLOCKED
              ? tr('employee_unblock')
              : tr('employee_block') }}
          </button>
          <button
            class="employees-page__nav-link employees-page__nav-link--danger"
            :disabled="selectedEmployee === null"
            @click="page.openDelete()"
          >
            {{ tr('employee_delete') }}
          </button>
        </div>

        <button class="employees-page__nav-link" @click="page.openCreateForm()">
          {{ tr('employee_add') }}
        </button>
      </div>

      <p v-if="employeesStore.isLoading">{{ LOADING_PLACEHOLDER }}</p>
      <p v-else-if="employeesStore.employees.length === 0">{{ tr('employees_empty') }}</p>

      <table v-else class="employees">
        <thead>
          <tr>
            <th>{{ tr('employee_name') }}</th>
            <th>{{ tr('employee_position') }}</th>
            <th>{{ tr('employee_phone') }}</th>
            <th>{{ tr('employee_status') }}</th>
            <th>{{ tr('employee_roles') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="employee in employeesStore.employees"
            :key="employee.id"
            tabindex="0"
            :aria-current="employee.id === selectedEmployeeId ? 'true' : undefined"
            :class="{ 'employees__row--selected': employee.id === selectedEmployeeId }"
            @click="page.select(employee.id)"
            @keydown.enter.prevent="page.select(employee.id)"
            @keydown.space.prevent="page.select(employee.id)"
          >
            <td>{{ fullName(employee) }}</td>
            <td>{{ employee.position }}</td>
            <td>{{ employee.phone }}</td>
            <td>{{ trStatus(employee.status) }}</td>
            <td>
              <span v-for="role in employee.roles" :key="role" class="employees__role">{{ role }}</span>
            </td>
          </tr>
        </tbody>
      </table>

      <nav v-if="employeesStore.totalPages > 1" class="employees-pager">
        <button
          class="employees-page__nav-link"
          :disabled="employeesStore.page <= 1"
          @click="page.goToPage(employeesStore.page - 1)"
        >
          {{ tr('employees_page_prev') }}
        </button>
        <span class="employees-pager__label">
          {{ tr('employees_page_of', employeesStore.page, employeesStore.totalPages) }}
        </span>
        <button
          class="employees-page__nav-link"
          :disabled="employeesStore.page >= employeesStore.totalPages"
          @click="page.goToPage(employeesStore.page + 1)"
        >
          {{ tr('employees_page_next') }}
        </button>
      </nav>

      <EmployeeFormDialog
        :open="isFormOpen"
        :initial="formInitial"
        :error="isFormOpen ? errorMessage : ''"
        :saving="isSaving"
        @submit="page.save($event)"
        @close="page.closeForm()"
      />

      <EmployeeRolesDialog
        :open="isRolesOpen"
        :employee="selectedEmployee"
        :roles="employeesStore.roles"
        :error="isRolesOpen ? errorMessage : ''"
        @save="page.saveRoles($event.roles, $event.adminLogin, $event.adminPassword)"
        @close="page.closeRoles()"
      />

      <EmployeeDeleteDialog
        :open="isDeleteOpen"
        :employee="selectedEmployee"
        :error="isDeleteOpen ? errorMessage : ''"
        @confirm="page.confirmDelete()"
        @close="page.closeDelete()"
      />
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

.employees-page__nav {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.employees-page__nav-link {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  color: var(--color-text);
  font-size: var(--font-size-base);
  text-decoration: none;
}

.employees-page__nav-link:hover:enabled {
  background: var(--color-bg);
}

.employees-page__nav-link:disabled {
  color: var(--color-text-muted);
  cursor: default;
}

.employees-page__nav-link--danger {
  color: var(--color-error);
  border-color: var(--color-error);
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

.employees-toolbar {
  display: flex;
  justify-content: space-between;
  gap: var(--space-2);
  margin-bottom: var(--space-3);
}

.employees-toolbar__group {
  display: flex;
  gap: var(--space-2);
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
}

.employees th {
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}

.employees tbody tr {
  cursor: pointer;
}

.employees tbody tr:hover {
  background: var(--color-bg);
}

.employees tbody tr:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: -2px;
}

.employees__role {
  display: inline-block;
  padding: var(--space-1) var(--space-2);
  margin-right: var(--space-1);
  background: var(--color-bg);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-small);
  white-space: nowrap;
}

.employees__row--selected,
.employees__row--selected:hover {
  background: var(--color-row-selected);
}

.employees-pager {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  margin-top: var(--space-3);
}

.employees-pager__label {
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}
</style>
