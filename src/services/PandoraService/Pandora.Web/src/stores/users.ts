import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/api/client'
import { EMPLOYEE_ENDPOINTS } from '@/constants/api'
import { EMPLOYEE_STORE_ID } from '@/constants/stores'
import { toRequestError, type RequestError } from '@/api/request-error'
import type { Employee } from './models/employee'
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
      employee.status = status
      return null
    } catch (error) {
      return toRequestError(error)
    }
  }

  return {
    employees,
    roles,
    isLoading,
    load,
    assignRoles,
    setStatus
  }
})
