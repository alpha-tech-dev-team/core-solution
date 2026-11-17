import { computed, onMounted, ref } from 'vue'
import { useViewError } from '@shared/composables/use-view-error'
import { useAuthStore } from '@shared/app/app-session'
import { useEmployeesStore } from '@/stores/employees'
import { EMPLOYEE_STATUS } from '@/constants/employee-status'
import { FIRST_PAGE } from '@shared/constants/pagination'
import { formatEmployeeFullName } from '@/models/employee'
import type { Employee } from '@/models/employee'
import { useEmployeeDelete } from './use-employee-delete'
import { useEmployeeForm } from './use-employee-form'
import { useEmployeeRoles } from './use-employee-roles'

export function useEmployeesView() {
  const authStore = useAuthStore()
  const employeesStore = useEmployeesStore()
  const { errorMessage, clearError, run } = useViewError()

  const selectedEmployeeId = ref<string | null>(null)

  const selectedEmployee = computed<Employee | null>(
    () =>
      employeesStore.employees.find((employee) => employee.id === selectedEmployeeId.value) ?? null,
  )

  const getSelected = () => selectedEmployee.value

  const form = useEmployeeForm(employeesStore, run, clearError)
  const roles = useEmployeeRoles(employeesStore, run, clearError, getSelected)
  const deletion = useEmployeeDelete(employeesStore, run, clearError, getSelected)

  async function load(pageNumber?: number): Promise<void> {
    await run(async () => {
      await Promise.all([employeesStore.load(pageNumber), employeesStore.loadRoles()])
    })
  }

  async function goToPage(pageNumber: number): Promise<void> {
    if (pageNumber < FIRST_PAGE || pageNumber > employeesStore.totalPages) return

    await load(pageNumber)
  }

  function isSelf(employee: Employee | null): boolean {
    return employee?.adminUsername != null && employee.adminUsername === authStore.username
  }

  function select(employeeId: string): void {
    selectedEmployeeId.value = employeeId
  }

  function openEditForm(): void {
    if (selectedEmployee.value !== null) form.openEdit(selectedEmployee.value)
  }

  async function toggleBlock(): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    const nextStatus =
      employee.status === EMPLOYEE_STATUS.BLOCKED ? EMPLOYEE_STATUS.ACTIVE : EMPLOYEE_STATUS.BLOCKED

    await run(() => employeesStore.setStatus(employee, nextStatus))
  }

  async function restore(): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    await run(() => employeesStore.setStatus(employee, EMPLOYEE_STATUS.ACTIVE))
  }

  async function toggleArchived(): Promise<void> {
    employeesStore.includeArchived = !employeesStore.includeArchived
    selectedEmployeeId.value = null
    await load(FIRST_PAGE)
  }

  onMounted(load)

  return {
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

    isFormOpen: form.isOpen,
    formInitial: form.initial,
    isSaving: form.isSaving,
    openCreateForm: form.openCreate,
    openEditForm,
    closeForm: form.close,
    save: form.save,

    isRolesOpen: roles.isOpen,
    openRoles: roles.open,
    closeRoles: roles.close,
    saveRoles: roles.save,

    isDeleteOpen: deletion.isOpen,
    openDelete: deletion.open,
    closeDelete: deletion.close,
    confirmDelete: deletion.confirm,
  }
}
