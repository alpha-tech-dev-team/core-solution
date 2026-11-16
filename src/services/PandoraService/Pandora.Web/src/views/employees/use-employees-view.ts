import { computed, onMounted, ref } from 'vue'
import { useViewError } from '@/composables/use-view-error'
import { useAuthStore } from '@shared/app/app-session'
import { useEmployeesStore } from '@/stores/employees'
import { EMPLOYEE_STATUS } from '@/constants/employee-status'
import { FIRST_PAGE } from '@/constants/pagination'
import { formatEmployeeFullName } from '@/models/employee'
import type { Employee } from '@/models/employee'
import type { EmployeeInput } from '@/models/employee-input'

export function useEmployeesView() {
  const authStore = useAuthStore()
  const employeesStore = useEmployeesStore()
  const { errorMessage, clearError, run } = useViewError()

  const selectedEmployeeId = ref<string | null>(null)
  const isFormOpen = ref(false)
  const isRolesOpen = ref(false)
  const isSaving = ref(false)
  const isDeleteOpen = ref(false)
  const formInitial = ref<EmployeeInput | null>(null)
  const editingEmployee = ref<Employee | null>(null)

  const selectedEmployee = computed<Employee | null>(
    () =>
      employeesStore.employees.find((employee) => employee.id === selectedEmployeeId.value) ?? null,
  )

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

  function openCreateForm(): void {
    editingEmployee.value = null
    formInitial.value = null
    clearError()
    isFormOpen.value = true
  }

  function openEditForm(): void {
    const employee = selectedEmployee.value
    if (employee === null) return

    editingEmployee.value = employee
    formInitial.value = {
      name: employee.name,
      surname: employee.surname,
      patronymic: employee.patronymic ?? '',
      position: employee.position,
      phone: employee.phone,
    }
    clearError()
    isFormOpen.value = true
  }

  function closeForm(): void {
    isFormOpen.value = false
  }

  async function save(input: EmployeeInput): Promise<void> {
    if (isSaving.value) return

    isSaving.value = true

    try {
      const isSaved = await run(async () => {
        if (editingEmployee.value === null) {
          await employeesStore.create(input)
          return
        }

        await employeesStore.update(editingEmployee.value, input)
      })

      if (isSaved) {
        isFormOpen.value = false
        editingEmployee.value = null
      }
    } finally {
      isSaving.value = false
    }
  }

  function openRoles(): void {
    if (selectedEmployee.value === null) return

    clearError()
    isRolesOpen.value = true
  }

  function closeRoles(): void {
    isRolesOpen.value = false
  }

  function openDelete(): void {
    if (selectedEmployee.value === null) return

    clearError()
    isDeleteOpen.value = true
  }

  function closeDelete(): void {
    isDeleteOpen.value = false
  }

  async function confirmDelete(): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    const isDeleted = await run(() => employeesStore.remove(employee))
    if (isDeleted) isDeleteOpen.value = false
  }

  async function saveRoles(
    roleNames: string[],
    adminLogin: string | null,
    adminPassword: string | null,
  ): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    const isSaved = await run(() =>
      employeesStore.assignRoles(employee, { roles: roleNames, adminLogin, adminPassword }),
    )
    if (isSaved) isRolesOpen.value = false
  }

  async function toggleBlock(): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    const nextStatus =
      employee.status === EMPLOYEE_STATUS.BLOCKED ? EMPLOYEE_STATUS.ACTIVE : EMPLOYEE_STATUS.BLOCKED

    await run(() => employeesStore.setStatus(employee, nextStatus))
  }

  onMounted(load)

  return {
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
    isSelf,
    select,
    goToPage,
    openCreateForm,
    openEditForm,
    closeForm,
    save,
    openRoles,
    closeRoles,
    openDelete,
    closeDelete,
    confirmDelete,
    saveRoles,
    toggleBlock,
  }
}
