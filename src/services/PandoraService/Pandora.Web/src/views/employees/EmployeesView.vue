<script setup lang="ts">
import { useTranslations } from '@shared/i18n/create-i18n'
import { EMPLOYEE_STATUS } from '@/constants/employee-status'
import { FIRST_PAGE } from '@/constants/pagination'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { ROUTE_NAMES } from '@/constants/routes'
import { LOADING_PLACEHOLDER } from '@shared/constants/ui'
import { useEmployeesView } from './use-employees-view'
import EmployeeDeleteDialog from './EmployeeDeleteDialog.vue'
import EmployeeFormDialog from './EmployeeFormDialog.vue'
import EmployeeRolesDialog from './EmployeeRolesDialog.vue'

const page = useEmployeesView()
const { tr, trStatus } = useTranslations()

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
  formatEmployeeFullName,
  logout,
} = page
</script>

<template>
  <div class="page">
    <header class="page__header">
      <h1>{{ tr(RESOURCE_KEYS.EMPLOYEES_TITLE) }}</h1>
      <nav class="page__nav">
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.HOME }">
          {{ tr(RESOURCE_KEYS.NAV_HOME) }}
        </RouterLink>
        <button class="btn btn--ghost btn--sm" @click="logout">
          {{ tr(RESOURCE_KEYS.AUTH_LOGOUT) }}
        </button>
      </nav>
    </header>

    <main class="page__body">
      <p v-if="errorMessage && !isFormOpen && !isRolesOpen" class="alert alert--error">
        {{ errorMessage }}
      </p>

      <div class="toolbar">
        <div class="toolbar__group">
          <button
            class="btn btn--ghost"
            :disabled="selectedEmployee === null"
            @click="page.openRoles()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_ROLES) }}
          </button>
          <button
            class="btn btn--ghost"
            :disabled="selectedEmployee === null"
            @click="page.openEditForm()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_EDIT) }}
          </button>
          <button
            class="btn btn--ghost"
            :disabled="selectedEmployee === null"
            @click="page.toggleBlock()"
          >
            {{
              selectedEmployee?.status === EMPLOYEE_STATUS.BLOCKED
                ? tr(RESOURCE_KEYS.EMPLOYEE_UNBLOCK)
                : tr(RESOURCE_KEYS.EMPLOYEE_BLOCK)
            }}
          </button>
          <button
            class="btn btn--danger"
            :disabled="selectedEmployee === null"
            @click="page.openDelete()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE) }}
          </button>
        </div>

        <button class="btn btn--primary" @click="page.openCreateForm()">
          {{ tr(RESOURCE_KEYS.EMPLOYEE_ADD) }}
        </button>
      </div>

      <p v-if="employeesStore.isLoading" class="text-muted">{{ LOADING_PLACEHOLDER }}</p>
      <p v-else-if="employeesStore.employees.length === 0" class="text-muted">
        {{ tr(RESOURCE_KEYS.EMPLOYEES_EMPTY) }}
      </p>

      <table v-else class="table">
        <thead>
          <tr>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_NAME) }}</th>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_POSITION) }}</th>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_PHONE) }}</th>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_STATUS) }}</th>
            <th>{{ tr(RESOURCE_KEYS.EMPLOYEE_ROLES) }}</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="employee in employeesStore.employees"
            :key="employee.id"
            tabindex="0"
            :aria-current="employee.id === selectedEmployeeId ? 'true' : undefined"
            :class="{ 'table__row--selected': employee.id === selectedEmployeeId }"
            @click="page.select(employee.id)"
            @keydown.enter.prevent="page.select(employee.id)"
            @keydown.space.prevent="page.select(employee.id)"
          >
            <td>{{ formatEmployeeFullName(employee) }}</td>
            <td>{{ employee.position }}</td>
            <td class="numeric">{{ employee.phone }}</td>
            <td>{{ trStatus(employee.status) }}</td>
            <td>
              <span v-for="role in employee.roles" :key="role" class="badge">{{ role }}</span>
            </td>
          </tr>
        </tbody>
      </table>

      <nav v-if="employeesStore.totalPages > 1" class="pager">
        <button
          class="btn btn--ghost btn--sm"
          :disabled="employeesStore.page <= FIRST_PAGE"
          @click="page.goToPage(employeesStore.page - 1)"
        >
          {{ tr(RESOURCE_KEYS.EMPLOYEES_PAGE_PREV) }}
        </button>
        <span class="pager__label">
          {{ tr(RESOURCE_KEYS.EMPLOYEES_PAGE_OF, employeesStore.page, employeesStore.totalPages) }}
        </span>
        <button
          class="btn btn--ghost btn--sm"
          :disabled="employeesStore.page >= employeesStore.totalPages"
          @click="page.goToPage(employeesStore.page + 1)"
        >
          {{ tr(RESOURCE_KEYS.EMPLOYEES_PAGE_NEXT) }}
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
        :is-self="page.isSelf(selectedEmployee)"
        :roles="employeesStore.roles"
        :error="isRolesOpen ? errorMessage : ''"
        @save="page.saveRoles($event.roles, $event.adminLogin, $event.adminPassword)"
        @close="page.closeRoles()"
      />

      <EmployeeDeleteDialog
        :open="isDeleteOpen"
        :employee="selectedEmployee"
        :is-self="page.isSelf(selectedEmployee)"
        :error="isDeleteOpen ? errorMessage : ''"
        @confirm="page.confirmDelete()"
        @close="page.closeDelete()"
      />
    </main>
  </div>
</template>
