import { ref, type Ref } from 'vue'
import type { Employee } from '@/models/employee'
import type { useEmployeesStore } from '@/stores/employees'

type EmployeesStore = ReturnType<typeof useEmployeesStore>
type RunAction = (action: () => Promise<void>) => Promise<boolean>

export interface EmployeeRolesState {
  isOpen: Ref<boolean>
  open(): void
  close(): void
  save(roles: string[], adminLogin: string | null, adminPassword: string | null): Promise<void>
}

export function useEmployeeRoles(
  employeesStore: EmployeesStore,
  run: RunAction,
  clearError: () => void,
  selectedEmployee: () => Employee | null,
): EmployeeRolesState {
  const isOpen = ref(false)

  function open(): void {
    if (selectedEmployee() === null) return

    clearError()
    isOpen.value = true
  }

  function close(): void {
    isOpen.value = false
  }

  async function save(
    roles: string[],
    adminLogin: string | null,
    adminPassword: string | null,
  ): Promise<void> {
    const employee = selectedEmployee()
    if (employee === null) return

    const isSaved = await run(() =>
      employeesStore.assignRoles(employee, { roles, adminLogin, adminPassword }),
    )
    if (isSaved) isOpen.value = false
  }

  return { isOpen, open, close, save }
}
