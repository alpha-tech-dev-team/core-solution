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
  const formInitial = ref<EmployeeInput | null>(null)
  const editingEmployee = ref<Employee | null>(null)

  const selectedEmployee = computed<Employee | null>(
    () => employeesStore.employees.find((employee) => employee.id === selectedEmployeeId.value) ?? null
  )

  async function load(): Promise<void> {
    errorMessage.value = ''
    const error = await employeesStore.load()
    if (error !== null) errorMessage.value = trRequestError(error)
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
  }

  function openRoles(): void {
    if (selectedEmployee.value === null) return

    errorMessage.value = ''
    isRolesOpen.value = true
  }

  function closeRoles(): void {
    isRolesOpen.value = false
  }

  async function toggleRole(role: string, checked: boolean): Promise<void> {
    const employee = selectedEmployee.value
    if (employee === null) return

    const roles = checked
      ? [...employee.roles, role]
      : employee.roles.filter((item) => item !== role)

    errorMessage.value = ''
    const error = await employeesStore.assignRoles(employee, roles)
    if (error === null) return

    errorMessage.value = trRequestError(error)
    await employeesStore.load()
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
    formInitial,
    fullName,
    logout,
    select,
    openCreateForm,
    openEditForm,
    closeForm,
    save,
    openRoles,
    closeRoles,
    toggleRole,
    toggleBlock
  }
}
