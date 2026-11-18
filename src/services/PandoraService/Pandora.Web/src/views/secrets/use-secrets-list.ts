import { computed, ref, type ComputedRef, type Ref } from 'vue'
import { FIRST_PAGE } from '@shared/constants/pagination'
import { fetchSecrets } from '@/api/secrets'
import type { SecretItem } from '@/models/secret-item'

export interface SecretsList {
  items: Ref<SecretItem[]>
  page: Ref<number>
  totalPages: ComputedRef<number>
  selectedId: Ref<string | null>
  selectedItem: ComputedRef<SecretItem | null>
  isLoading: Ref<boolean>
  load(): Promise<void>
  changePage(target: number): Promise<void>
  select(item: SecretItem): void
  resetSelection(): void
}

export function useSecretsList(
  pageSize: Ref<number>,
  total: Ref<number>,
  run: (action: () => Promise<void>) => Promise<boolean>,
): SecretsList {
  const items = ref<SecretItem[]>([])
  const page = ref(FIRST_PAGE)
  const selectedId = ref<string | null>(null)
  const isLoading = ref(false)

  const totalPages = computed(() => Math.max(FIRST_PAGE, Math.ceil(total.value / pageSize.value)))
  const selectedItem = computed(
    () => items.value.find((item) => item.id === selectedId.value) ?? null,
  )

  async function load(): Promise<void> {
    isLoading.value = true

    try {
      await run(async () => {
        let result = await fetchSecrets(page.value, pageSize.value)

        while (result.items.length === 0 && result.total > 0 && page.value > FIRST_PAGE) {
          page.value -= 1
          result = await fetchSecrets(page.value, pageSize.value)
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
    selectedId.value = null
    await load()
  }

  function select(item: SecretItem): void {
    selectedId.value = selectedId.value === item.id ? null : item.id
  }

  return {
    items,
    page,
    totalPages,
    selectedId,
    selectedItem,
    isLoading,
    load,
    changePage,
    select,
    resetSelection: () => {
      selectedId.value = null
    },
  }
}
