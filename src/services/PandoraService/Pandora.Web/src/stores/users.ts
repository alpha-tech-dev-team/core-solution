import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/api/client'
import { EMPLOYEE_ENDPOINTS } from '@/constants/api'
import { EMPLOYEE_STORE_ID } from '@/constants/stores'
import { toRequestError, type RequestError } from '@/api/request-error'
import type { Employee } from './models/employee'
import type { EmployeeInput } from './models/employee-input'
import type { EmployeeStatus } from './models/employee-status'

export const useEmployeesStore = defineStore(EMPLOYEE_STORE_ID, () => {
  const employees = ref<Employee[]>([])
  const roles = ref<string[]>([])
  const isLoading = ref(false)

  async function load(): Promise<RequestError | null> {
    isLoading.value = true
    try {
      const [employeesResponse, rolesResponse] = await Promise.all([
        api.get<Employee[]>(EMPLOYEE_ENDPOINTS.LIST),
        api.get<string[]>(EMPLOYEE_ENDPOINTS.ROLES)
      ])
      employees.value = employeesResponse.data
      roles.value = rolesResponse.data
      return null
    } catch (error) {
      return toRequestError(error)
    } finally {
      isLoading.value = false
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

  async function assignRoles(employee: Employee, roleNames: string[]): Promise<RequestError | null> {
    try {
      await api.put(EMPLOYEE_ENDPOINTS.ROLES_OF(employee.id), { roles: roleNames })
      employee.roles = roleNames
      return null
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
    isLoading,
    load,
    create,
    update,
    assignRoles,
    setStatus
  }
})
