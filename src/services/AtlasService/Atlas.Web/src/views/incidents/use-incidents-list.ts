import { computed, ref, type ComputedRef, type Ref } from 'vue'
import { FIRST_PAGE } from '@shared/constants/pagination'
import { fetchIncidents } from '@/api/incidents'
import type { IncidentItem } from '@/models/incident-item'

export interface IncidentsList {
  items: Ref<IncidentItem[]>
  page: Ref<number>
  totalPages: ComputedRef<number>
  isLoading: Ref<boolean>
  load(): Promise<void>
  changePage(target: number): Promise<void>
}

export function useIncidentsList(
  pageSize: Ref<number>,
  total: Ref<number>,
  run: (action: () => Promise<void>) => Promise<boolean>,
): IncidentsList {
  const items = ref<IncidentItem[]>([])
  const page = ref(FIRST_PAGE)
  const isLoading = ref(false)

  const totalPages = computed(() => Math.max(FIRST_PAGE, Math.ceil(total.value / pageSize.value)))

  async function load(): Promise<void> {
    isLoading.value = true

    try {
      await run(async () => {
        let result = await fetchIncidents(page.value, pageSize.value)

        while (result.items.length === 0 && result.total > 0 && page.value > FIRST_PAGE) {
          page.value -= 1
          result = await fetchIncidents(page.value, pageSize.value)
        }

        items.value = result.items
        page.value = result.page
        pageSize.value = result.pageSize
        total.value = result.total
      })
    } finally {
      isLoading.value = false
    }
  }

  async function changePage(target: number): Promise<void> {
    if (target < FIRST_PAGE || target > totalPages.value || target === page.value) return

    page.value = target
    await load()
  }

  return {
    items,
    page,
    totalPages,
    isLoading,
    load,
    changePage,
  }
}
