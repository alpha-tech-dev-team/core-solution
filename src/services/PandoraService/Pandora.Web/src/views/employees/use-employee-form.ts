import { ref, type Ref } from 'vue'
import type { EmployeeInput } from '@/models/employee-input'
import type { Employee } from '@/models/employee'
import type { useEmployeesStore } from '@/stores/employees'

type EmployeesStore = ReturnType<typeof useEmployeesStore>
type RunAction = (action: () => Promise<void>) => Promise<boolean>

export interface EmployeeFormState {
  isOpen: Ref<boolean>
  isSaving: Ref<boolean>
  initial: Ref<EmployeeInput | null>
  openCreate(): void
  openEdit(employee: Employee): void
  close(): void
  save(input: EmployeeInput): Promise<void>
}

export function useEmployeeForm(
  employeesStore: EmployeesStore,
  run: RunAction,
  clearError: () => void,
): EmployeeFormState {
  const isOpen = ref(false)
  const isSaving = ref(false)
  const initial = ref<EmployeeInput | null>(null)
  const editing = ref<Employee | null>(null)

  function openCreate(): void {
    editing.value = null
    initial.value = null
    clearError()
    isOpen.value = true
  }

  function openEdit(employee: Employee): void {
    editing.value = employee
    initial.value = {
      name: employee.name,
      surname: employee.surname,
      patronymic: employee.patronymic ?? '',
      position: employee.position,
      phone: employee.phone,
    }
    clearError()
    isOpen.value = true
  }

  function close(): void {
    isOpen.value = false
  }

  async function save(input: EmployeeInput): Promise<void> {
    if (isSaving.value) return

    isSaving.value = true

    try {
      const current = editing.value
      const isSaved = await run(async () => {
        if (current === null) {
          await employeesStore.create(input)
          return
        }

        await employeesStore.update(current, input)
      })

      if (isSaved) {
        isOpen.value = false
        editing.value = null
      }
    } finally {
      isSaving.value = false
    }
  }

  return { isOpen, isSaving, initial, openCreate, openEdit, close, save }
}
