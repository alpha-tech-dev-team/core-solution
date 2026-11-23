import { computed, ref, type ComputedRef, type Ref } from 'vue'
import { FIRST_PAGE } from '@shared/constants/pagination'
import { SECRETS_PAGE_SIZE } from '@/constants/pagination'
import { fetchEmployeeSecrets, grantSecretAccess, revokeSecretAccess } from '@/api/employee-secrets'
import type { Employee } from '@/models/employee'
import type { EmployeeSecret } from '@/models/employee-secret'

type RunAction = (action: () => Promise<void>) => Promise<boolean>

export interface EmployeeSecretsState {
  isOpen: Ref<boolean>
  items: Ref<EmployeeSecret[]>
  page: Ref<number>
  totalPages: ComputedRef<number>
  isLoading: Ref<boolean>
  savingItemId: Ref<string | null>
  open(): void
  close(): void
  changePage(target: number): Promise<void>
  toggle(item: EmployeeSecret): Promise<void>
}

export function useEmployeeSecrets(
  run: RunAction,
  clearError: () => void,
  selectedEmployee: () => Employee | null,
): EmployeeSecretsState {
  const isOpen = ref(false)
  const items = ref<EmployeeSecret[]>([])
  const page = ref(FIRST_PAGE)
  const pageSize = ref(SECRETS_PAGE_SIZE)
  const total = ref(0)
  const isLoading = ref(false)
  const savingItemId = ref<string | null>(null)

  let requestId = 0

  const totalPages = computed(() => Math.max(FIRST_PAGE, Math.ceil(total.value / pageSize.value)))

  function reset(): void {
    requestId += 1
    items.value = []
    page.value = FIRST_PAGE
    pageSize.value = SECRETS_PAGE_SIZE
    total.value = 0
    isLoading.value = false
    savingItemId.value = null
  }

  async function load(telegramUserId: number, pageNumber: number): Promise<void> {
    requestId += 1
    const currentRequestId = requestId
    isLoading.value = true

    await run(async () => {
      const result = await fetchEmployeeSecrets(telegramUserId, pageNumber, pageSize.value)

      if (currentRequestId !== requestId) return

      items.value = result.items
      page.value = result.page
      pageSize.value = result.pageSize
      total.value = result.total
    })

    if (currentRequestId === requestId) isLoading.value = false
  }

  function open(): void {
    const employee = selectedEmployee()
    if (employee === null) return

    clearError()
    reset()
    isOpen.value = true

    if (employee.telegramUserId === null) return

    void load(employee.telegramUserId, FIRST_PAGE)
  }

  function close(): void {
    isOpen.value = false
    reset()
  }

  async function changePage(target: number): Promise<void> {
    const employee = selectedEmployee()
    if (employee === null || employee.telegramUserId === null) return
    if (target < FIRST_PAGE || target > totalPages.value || target === page.value) return

    await load(employee.telegramUserId, target)
  }

  async function toggle(item: EmployeeSecret): Promise<void> {
    const employee = selectedEmployee()
    if (employee === null || employee.telegramUserId === null || savingItemId.value !== null) return

    const telegramUserId = employee.telegramUserId
    const previousState = item.isGranted

    savingItemId.value = item.id
    item.isGranted = !previousState

    const isSaved = await run(async () => {
      if (previousState) {
        await revokeSecretAccess(telegramUserId, item.id)
        return
      }

      await grantSecretAccess(telegramUserId, item.id)
    })

    if (!isSaved) item.isGranted = previousState

    savingItemId.value = null
  }

  return {
    isOpen,
    items,
    page,
    totalPages,
    isLoading,
    savingItemId,
    open,
    close,
    changePage,
    toggle,
  }
}
