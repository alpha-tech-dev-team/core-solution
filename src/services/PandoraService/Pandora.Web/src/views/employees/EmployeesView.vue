<script setup lang="ts">
import { EMPLOYEE_STATUS } from '@/constants/employee-status'
import { RESOURCE_KEYS } from '@/constants/i18n'
import { FIRST_PAGE } from '@/constants/pagination'
import { ROUTE_NAMES } from '@/constants/routes'
import { LOADING_PLACEHOLDER } from '@/constants/ui'
import { tr, trStatus } from '@/i18n'
import { useEmployeesView } from './use-employees-view'
import EmployeeDeleteDialog from './EmployeeDeleteDialog.vue'
import EmployeeFormDialog from './EmployeeFormDialog.vue'
import EmployeeRolesDialog from './EmployeeRolesDialog.vue'

const page = useEmployeesView()

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
  <div class="employees-page">
    <header class="employees-page__header">
      <h1>{{ tr(RESOURCE_KEYS.EMPLOYEES_TITLE) }}</h1>
      <nav class="employees-page__nav">
        <RouterLink class="employees-page__nav-link" :to="{ name: ROUTE_NAMES.HOME }">
          {{ tr(RESOURCE_KEYS.NAV_HOME) }}
        </RouterLink>
        <button class="employees-page__nav-link" @click="logout">
          {{ tr(RESOURCE_KEYS.AUTH_LOGOUT) }}
        </button>
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
            {{ tr(RESOURCE_KEYS.EMPLOYEE_ROLES) }}
          </button>
          <button
            class="employees-page__nav-link"
            :disabled="selectedEmployee === null"
            @click="page.openEditForm()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_EDIT) }}
          </button>
          <button
            class="employees-page__nav-link"
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
            class="employees-page__nav-link employees-page__nav-link--danger"
            :disabled="selectedEmployee === null"
            @click="page.openDelete()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE) }}
          </button>
        </div>

        <button class="employees-page__nav-link" @click="page.openCreateForm()">
          {{ tr(RESOURCE_KEYS.EMPLOYEE_ADD) }}
        </button>
      </div>

      <p v-if="employeesStore.isLoading">{{ LOADING_PLACEHOLDER }}</p>
      <p v-else-if="employeesStore.employees.length === 0">
        {{ tr(RESOURCE_KEYS.EMPLOYEES_EMPTY) }}
      </p>

      <table v-else class="employees">
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
            :class="{ 'employees__row--selected': employee.id === selectedEmployeeId }"
            @click="page.select(employee.id)"
            @keydown.enter.prevent="page.select(employee.id)"
            @keydown.space.prevent="page.select(employee.id)"
          >
            <td>{{ formatEmployeeFullName(employee) }}</td>
            <td>{{ employee.position }}</td>
            <td>{{ employee.phone }}</td>
            <td>{{ trStatus(employee.status) }}</td>
            <td>
              <span v-for="role in employee.roles" :key="role" class="employees__role">
                {{ role }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>

      <nav v-if="employeesStore.totalPages > 1" class="employees-pager">
        <button
          class="employees-page__nav-link"
          :disabled="employeesStore.page <= FIRST_PAGE"
          @click="page.goToPage(employeesStore.page - 1)"
        >
          {{ tr(RESOURCE_KEYS.EMPLOYEES_PAGE_PREV) }}
        </button>
        <span class="employees-pager__label">
          {{ tr(RESOURCE_KEYS.EMPLOYEES_PAGE_OF, employeesStore.page, employeesStore.totalPages) }}
        </span>
        <button
          class="employees-page__nav-link"
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

<style scoped src="./employees-view.css"></style>
