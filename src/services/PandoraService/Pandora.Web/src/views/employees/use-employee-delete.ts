import { ref, type Ref } from 'vue'
import type { Employee } from '@/models/employee'
import type { useEmployeesStore } from '@/stores/employees'

type EmployeesStore = ReturnType<typeof useEmployeesStore>
type RunAction = (action: () => Promise<void>) => Promise<boolean>

export interface EmployeeDeleteState {
  isOpen: Ref<boolean>
  open(): void
  close(): void
  confirm(): Promise<void>
}

export function useEmployeeDelete(
  employeesStore: EmployeesStore,
  run: RunAction,
  clearError: () => void,
  selectedEmployee: () => Employee | null,
): EmployeeDeleteState {
  const isOpen = ref(false)

  function open(): void {
    if (selectedEmployee() === null) return

    clearError()
    isOpen.value = true
  }

  function close(): void {
    isOpen.value = false
  }

  async function confirm(): Promise<void> {
    const employee = selectedEmployee()
    if (employee === null) return

    const isDeleted = await run(() => employeesStore.remove(employee))
    if (isDeleted) isOpen.value = false
  }

  return { isOpen, open, close, confirm }
}
