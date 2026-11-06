import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useEmployeesStore } from '@/stores/users'
import { EMPLOYEE_STATUS } from '@/stores/models/employee-status'
import type { Employee } from '@/stores/models/employee'
import type { EmployeeInput } from '@/stores/models/employee-input'
import { trRequestError } from '@/i18n'
import { ROUTES } from '@/constants/routes'

export function useEmployeesPage() {
  const router = useRouter()
  const authStore = useAuthStore()
  const employeesStore = useEmployeesStore()
  const errorMessage = ref('')
  const selectedEmployeeId = ref<string | null>(null)
  const isFormOpen = ref(false)
  const isRolesOpen = ref(false)
  const isSaving = ref(false)
  const isDeleteOpen = ref(false)
  const formInitial = ref<EmployeeInput | null>(null)
  const editingEmployee = ref<Employee | null>(null)

  const selectedEmployee = computed<Employee | null>(
    () => employeesStore.employees.find((employee) => employee.id === selectedEmployeeId.value) ?? null
  )

  async function load(pageNumber?: number): Promise<void> {
    errorMessage.value = ''
    const error = await employeesStore.load(pageNumber)
    if (error !== null) errorMessage.value = trRequestError(error)
  }

  async function goToPage(pageNumber: number): Promise<void> {
    if (pageNumber < 1 || pageNumber > employeesStore.totalPages) return

    await load(pageNumber)
  }

  function fullName(employee: Employee): string {
    return [employee.surname, employee.name, employee.patronymic].filter(Boolean).join(' ')
  }

  async function logout(): Promise<void> {
    await authStore.logout()
    await router.push(ROUTES.LOGIN)
  }

  function select(employeeId: string): void {
    selectedEmployeeId.value = employeeId
  }

  function openCreateForm(): void {
    editingEmployee.value = null
    formInitial.value = null
    errorMessage.value = ''
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
      phone: employee.phone
    }
    errorMessage.value = ''
    isFormOpen.value = true
  }

  function closeForm(): void {
    isFormOpen.value = false
  }

  async function save(input: EmployeeInput): Promise<void> {
    if (isSaving.value) return

    isSaving.value = true
    try {
      errorMessage.value = ''
      const error = editingEmployee.value === null
        ? await employeesStore.create(input)
        : await employeesStore.update(editingEmployee.value, input)

      if (error !== null) {
        errorMessage.value = trRequestError(error)
        return
      }

      isFormOpen.value = false
      editingEmployee.value = null
    } finally {
      isSaving.value = false
    }
  }

  function openRoles(): void {
    if (selectedEmployee.value === null) return

    errorMessage.value = ''
    isRolesOpen.value = true
  }

  function closeRoles(): void {
    isRolesOpen.value = false
  }

  function openDelete(): void {
    if (selectedEmployee.value === null) return

    errorMessage.value = ''
    isDeleteOpen.value = true
  }

  function closeDelete(): void {
    isDeleteOpen.value = false
  }

  async function confirmDelete(): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    errorMessage.value = ''
    const error = await employeesStore.remove(employee)
    if (error !== null) {
      errorMessage.value = trRequestError(error)
      return
    }

    isDeleteOpen.value = false
  }

  async function saveRoles(roleNames: string[], adminLogin: string | null, adminPassword: string | null): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    errorMessage.value = ''
    const error = await employeesStore.assignRoles(employee, roleNames, adminLogin, adminPassword)
    if (error !== null) {
      errorMessage.value = trRequestError(error)
      return
    }

    isRolesOpen.value = false
  }

  async function toggleBlock(): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    const nextStatus = employee.status === EMPLOYEE_STATUS.BLOCKED
      ? EMPLOYEE_STATUS.ACTIVE
      : EMPLOYEE_STATUS.BLOCKED

    errorMessage.value = ''
    const error = await employeesStore.setStatus(employee, nextStatus)
    if (error === null) return

    errorMessage.value = trRequestError(error)
    await employeesStore.load()
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
    fullName,
    logout,
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
    toggleBlock
  }
}
