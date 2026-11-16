import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import api from '@/api/client'
import { EMPLOYEE_ENDPOINTS } from '@/constants/api'
import { EMPLOYEE_STORE_ID } from '@/constants/stores'
import { toRequestError, type RequestError } from '@/api/request-error'
import type { Employee } from './models/employee'
import type { EmployeeInput } from './models/employee-input'
import type { EmployeeStatus } from './models/employee-status'
import type { EmployeesPage } from './models/employees-page'

export const useEmployeesStore = defineStore(EMPLOYEE_STORE_ID, () => {
  const employees = ref<Employee[]>([])
  const roles = ref<string[]>([])
  const page = ref(1)
  const pageSize = ref(0)
  const total = ref(0)
  const totalPages = computed(() => pageSize.value === 0 ? 0 : Math.ceil(total.value / pageSize.value))
  const isLoading = ref(false)
  const rolesLoaded = ref(false)
  const includeArchived = ref(false)

  async function load(pageNumber: number = page.value): Promise<RequestError | null> {
    isLoading.value = true
    try {
      const response = await api.get<EmployeesPage>(EMPLOYEE_ENDPOINTS.LIST, {
        params: { page: pageNumber, includeArchived: includeArchived.value }
      })
      employees.value = response.data.items
      page.value = response.data.page
      pageSize.value = response.data.pageSize
      total.value = response.data.total
      return null
    } catch (error) {
      return toRequestError(error)
    } finally {
      isLoading.value = false
    }
  }

  async function loadRoles(): Promise<RequestError | null> {
    if (rolesLoaded.value) return null

    try {
      const response = await api.get<string[]>(EMPLOYEE_ENDPOINTS.ROLES)
      roles.value = response.data
      rolesLoaded.value = true
      return null
    } catch (error) {
      return toRequestError(error)
    }
  }

  async function create(input: EmployeeInput): Promise<RequestError | null> {
    try {
      await api.post(EMPLOYEE_ENDPOINTS.LIST, input)
      return await load()
    } catch (error) {
      return toRequestError(error)
    }
  }

  async function update(employee: Employee, input: EmployeeInput): Promise<RequestError | null> {
    try {
      await api.put(EMPLOYEE_ENDPOINTS.BY_ID(employee.id), input)
      return await load()
    } catch (error) {
      return toRequestError(error)
    }
  }

  async function remove(employee: Employee): Promise<RequestError | null> {
    try {
      await api.delete(EMPLOYEE_ENDPOINTS.BY_ID(employee.id))
      return await load()
    } catch (error) {
      return toRequestError(error)
    }
  }

  async function assignRoles(
    employee: Employee,
    roleNames: string[],
    adminLogin: string | null,
    adminPassword: string | null
  ): Promise<RequestError | null> {
    try {
      await api.put(EMPLOYEE_ENDPOINTS.ROLES_OF(employee.id), { roles: roleNames, adminLogin, adminPassword })
      return await load()
    } catch (error) {
      return toRequestError(error)
    }
  }

  async function setStatus(employee: Employee, status: EmployeeStatus): Promise<RequestError | null> {
    try {
      await api.put(EMPLOYEE_ENDPOINTS.STATUS_OF(employee.id), { status })
      return await load()
    } catch (error) {
      return toRequestError(error)
    }
  }

  return {
    employees,
    roles,
    page,
    pageSize,
    total,
    totalPages,
    isLoading,
    includeArchived,
    load,
    loadRoles,
    create,
    update,
    remove,
    assignRoles,
    setStatus
  }
})
