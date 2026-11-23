<script setup lang="ts">
import LogoutButton from '@shared/components/LogoutButton.vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import { LOADING_PLACEHOLDER } from '@shared/constants/ui'
import { EMPLOYEE_STATUS } from '@/constants/employee-status'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { ROUTE_NAMES } from '@/constants/routes'
import PaginationNav from '@shared/components/PaginationNav.vue'
import { useEmployeesView } from './use-employees-view'
import EmployeeDeleteDialog from './EmployeeDeleteDialog.vue'
import EmployeeFormDialog from './EmployeeFormDialog.vue'
import EmployeeRolesDialog from './EmployeeRolesDialog.vue'
import EmployeeSecretsDialog from './EmployeeSecretsDialog.vue'

const {
  employeesStore,
  errorMessage,
  selectedEmployee,
  selectedEmployeeId,
  formatEmployeeFullName,
  isSelf,
  select,
  goToPage,
  toggleBlock,
  restore,
  toggleArchived,
  isFormOpen,
  formInitial,
  isSaving,
  openCreateForm,
  openEditForm,
  closeForm,
  save,
  isRolesOpen,
  openRoles,
  closeRoles,
  saveRoles,
  isDeleteOpen,
  openDelete,
  closeDelete,
  confirmDelete,
  isSecretsOpen,
  employeeSecrets,
  isSecretsLoading,
  savingSecretId,
  secretsPage,
  secretsTotalPages,
  openSecrets,
  closeSecrets,
  changeSecretsPage,
  toggleSecretAccess,
} = useEmployeesView()

const { tr, trStatus } = useTranslations()
</script>

<template>
  <div class="page">
    <header class="page__header">
      <h1>{{ tr(RESOURCE_KEYS.EMPLOYEES_TITLE) }}</h1>
      <nav class="page__nav">
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.HOME }">
          {{ tr(RESOURCE_KEYS.NAV_HOME) }}
        </RouterLink>
        <LogoutButton />
      </nav>
    </header>

    <main class="page__body">
      <p
        v-if="errorMessage && !isFormOpen && !isRolesOpen && !isSecretsOpen"
        class="alert alert--error"
      >
        {{ errorMessage }}
      </p>

      <div class="toolbar">
        <div class="toolbar__group">
          <button class="btn btn--ghost" :disabled="selectedEmployee === null" @click="openRoles()">
            {{ tr(RESOURCE_KEYS.EMPLOYEE_ROLES) }}
          </button>
          <button
            class="btn btn--ghost"
            :disabled="selectedEmployee === null"
            @click="openSecrets()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_SECRETS) }}
          </button>
          <button
            class="btn btn--ghost"
            :disabled="selectedEmployee === null"
            @click="openEditForm()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_EDIT) }}
          </button>
          <button
            class="btn btn--ghost"
            :disabled="
              selectedEmployee === null ||
              isSelf(selectedEmployee) ||
              selectedEmployee.status === EMPLOYEE_STATUS.ARCHIVED
            "
            @click="toggleBlock()"
          >
            {{
              selectedEmployee?.status === EMPLOYEE_STATUS.BLOCKED
                ? tr(RESOURCE_KEYS.EMPLOYEE_UNBLOCK)
                : tr(RESOURCE_KEYS.EMPLOYEE_BLOCK)
            }}
          </button>
          <button
            class="btn btn--ghost"
            :disabled="selectedEmployee?.status !== EMPLOYEE_STATUS.ARCHIVED"
            @click="restore()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_RESTORE) }}
          </button>
          <button
            class="btn btn--danger"
            :disabled="
              selectedEmployee === null ||
              isSelf(selectedEmployee) ||
              selectedEmployee.status === EMPLOYEE_STATUS.ARCHIVED
            "
            @click="openDelete()"
          >
            {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE) }}
          </button>
        </div>

        <div class="toolbar__group">
          <label class="checkbox">
            <input
              type="checkbox"
              :checked="employeesStore.includeArchived"
              @change="toggleArchived()"
            />
            {{ tr(RESOURCE_KEYS.EMPLOYEES_INCLUDE_ARCHIVED) }}
          </label>
          <button class="btn btn--primary" @click="openCreateForm()">
            {{ tr(RESOURCE_KEYS.EMPLOYEE_ADD) }}
          </button>
        </div>
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
            @click="select(employee.id)"
            @keydown.enter.prevent="select(employee.id)"
            @keydown.space.prevent="select(employee.id)"
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

      <PaginationNav
        :page="employeesStore.page"
        :total-pages="employeesStore.totalPages"
        :prev-label="tr(RESOURCE_KEYS.EMPLOYEES_PAGE_PREV)"
        :next-label="tr(RESOURCE_KEYS.EMPLOYEES_PAGE_NEXT)"
        :info-label="
          tr(RESOURCE_KEYS.EMPLOYEES_PAGE_OF, employeesStore.page, employeesStore.totalPages)
        "
        @change="goToPage($event)"
      />

      <EmployeeFormDialog
        :open="isFormOpen"
        :initial="formInitial"
        :error="isFormOpen ? errorMessage : ''"
        :saving="isSaving"
        @submit="save($event)"
        @close="closeForm()"
      />

      <EmployeeRolesDialog
        :open="isRolesOpen"
        :employee="selectedEmployee"
        :is-self="isSelf(selectedEmployee)"
        :roles="employeesStore.roles"
        :error="isRolesOpen ? errorMessage : ''"
        @save="saveRoles($event.roles, $event.adminLogin, $event.adminPassword)"
        @close="closeRoles()"
      />

      <EmployeeDeleteDialog
        :open="isDeleteOpen"
        :employee="selectedEmployee"
        :is-self="isSelf(selectedEmployee)"
        :error="isDeleteOpen ? errorMessage : ''"
        @confirm="confirmDelete()"
        @close="closeDelete()"
      />

      <EmployeeSecretsDialog
        :open="isSecretsOpen"
        :employee="selectedEmployee"
        :secrets="employeeSecrets"
        :is-loading="isSecretsLoading"
        :saving-item-id="savingSecretId"
        :error="isSecretsOpen ? errorMessage : ''"
        :page="secretsPage"
        :total-pages="secretsTotalPages"
        @toggle="toggleSecretAccess($event)"
        @page-change="changeSecretsPage($event)"
        @close="closeSecrets()"
      />
    </main>
  </div>
</template>
