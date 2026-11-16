import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  assignEmployeeRoles,
  createEmployee,
  deleteEmployee,
  fetchEmployeeRoles,
  fetchEmployees,
  updateEmployee,
  updateEmployeeStatus,
} from '@/api/employees'
import { EMPLOYEE_STORE_ID } from '@/constants/stores'
import { FIRST_PAGE } from '@shared/constants/pagination'
import type { Employee } from '@/models/employee'
import type { EmployeeInput } from '@/models/employee-input'
import type { EmployeeRolesPayload } from '@/models/employee-roles-payload'
import type { EmployeeStatus } from '@/models/employee-status'

export const useEmployeesStore = defineStore(EMPLOYEE_STORE_ID, () => {
  const employees = ref<Employee[]>([])
  const roles = ref<string[]>([])
  const page = ref(FIRST_PAGE)
  const pageSize = ref(0)
  const total = ref(0)
  const isLoading = ref(false)
  const rolesLoaded = ref(false)

  const totalPages = computed(() =>
    pageSize.value === 0 ? 0 : Math.ceil(total.value / pageSize.value),
  )

  async function load(pageNumber: number = page.value): Promise<void> {
    isLoading.value = true

    try {
      const response = await fetchEmployees(pageNumber)

      employees.value = response.items
      page.value = response.page
      pageSize.value = response.pageSize
      total.value = response.total
    } finally {
      isLoading.value = false
    }
  }

  async function loadRoles(): Promise<void> {
    if (rolesLoaded.value) return

    roles.value = await fetchEmployeeRoles()
    rolesLoaded.value = true
  }

  async function create(input: EmployeeInput): Promise<void> {
    await createEmployee(input)
    await load()
  }

  async function update(employee: Employee, input: EmployeeInput): Promise<void> {
    await updateEmployee(employee.id, input)
    await load()
  }

  async function remove(employee: Employee): Promise<void> {
    await deleteEmployee(employee.id)
    await load()
  }

  async function assignRoles(employee: Employee, payload: EmployeeRolesPayload): Promise<void> {
    await assignEmployeeRoles(employee.id, payload)
    await load()
  }

  async function setStatus(employee: Employee, status: EmployeeStatus): Promise<void> {
    await updateEmployeeStatus(employee.id, status)
    await load()
  }

  return {
    employees,
    roles,
    page,
    pageSize,
    total,
    totalPages,
    isLoading,
    load,
    loadRoles,
    create,
    update,
    remove,
    assignRoles,
    setStatus,
  }
})
