import { computed, ref, type ComputedRef, type Ref } from 'vue'
import { FIRST_PAGE } from '@shared/constants/pagination'
import { useTranslations } from '@shared/i18n/create-i18n'
import { fetchAuditEntries } from '@/api/audit'
import { AUDIT_PAGE_SIZE } from '@/constants/pagination'
import type { AuditEntry, AuditQuery } from '@/models/audit-entry'

export interface AuditEntries {
  entries: Ref<AuditEntry[]>
  page: Ref<number>
  pageSize: Ref<number>
  total: Ref<number>
  totalPages: ComputedRef<number>
  isLoading: Ref<boolean>
  errorMessage: Ref<string>
  load(): Promise<void>
  changePage(target: number): Promise<void>
  prepend(entry: AuditEntry): void
  increaseTotal(): void
  dispose(): void
}

export function useAuditEntries(query: AuditQuery): AuditEntries {
  const { trError } = useTranslations()
  const entries = ref<AuditEntry[]>([])
  const page = ref(FIRST_PAGE)
  const pageSize = ref(AUDIT_PAGE_SIZE)
  const total = ref(0)
  const isLoading = ref(false)
  const errorMessage = ref('')
  let requestId = 0

  const totalPages = computed(() => Math.max(FIRST_PAGE, Math.ceil(total.value / pageSize.value)))

  async function load(): Promise<void> {
    const currentRequestId = ++requestId
    isLoading.value = true
    errorMessage.value = ''

    try {
      const result = await fetchAuditEntries(page.value, pageSize.value, query)
      if (currentRequestId !== requestId) return

      const lastPage = Math.max(FIRST_PAGE, Math.ceil(result.total / result.pageSize))
      if (result.page > lastPage) {
        page.value = lastPage
        await load()
        return
      }

      entries.value = result.items
      page.value = result.page
      pageSize.value = result.pageSize
      total.value = result.total
    } catch (error) {
      if (currentRequestId === requestId) errorMessage.value = trError(error)
    } finally {
      if (currentRequestId === requestId) isLoading.value = false
    }
  }

  async function changePage(target: number): Promise<void> {
    if (target < FIRST_PAGE || target > totalPages.value || target === page.value) return

    page.value = target
    await load()
  }

  function prepend(entry: AuditEntry): void {
    entries.value = [entry, ...entries.value].slice(0, pageSize.value)
  }

  function increaseTotal(): void {
    total.value += 1
  }

  return {
    entries,
    page,
    pageSize,
    total,
    totalPages,
    isLoading,
    errorMessage,
    load,
    changePage,
    prepend,
    increaseTotal,
    dispose: () => {
      requestId += 1
    },
  }
}
