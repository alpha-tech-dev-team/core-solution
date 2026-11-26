import { computed, ref, type ComputedRef, type Ref } from 'vue'
import { FIRST_PAGE } from '@shared/constants/pagination'
import { fetchRooms } from '@/api/rooms'
import type { RoomItem } from '@/models/room-item'

export interface RoomsList {
  items: Ref<RoomItem[]>
  page: Ref<number>
  totalPages: ComputedRef<number>
  isLoading: Ref<boolean>
  selectedId: Ref<string | null>
  selectedItem: ComputedRef<RoomItem | null>
  load(): Promise<void>
  changePage(target: number): Promise<void>
  select(item: RoomItem): void
  resetSelection(): void
}

export function useRoomsList(
  pageSize: Ref<number>,
  total: Ref<number>,
  run: (action: () => Promise<void>) => Promise<boolean>,
): RoomsList {
  const items = ref<RoomItem[]>([])
  const page = ref(FIRST_PAGE)
  const isLoading = ref(false)
  const selectedId = ref<string | null>(null)

  const totalPages = computed(() => Math.max(FIRST_PAGE, Math.ceil(total.value / pageSize.value)))
  const selectedItem = computed(
    () => items.value.find((item) => item.id === selectedId.value) ?? null,
  )

  async function load(): Promise<void> {
    isLoading.value = true

    try {
      await run(async () => {
        let result = await fetchRooms(page.value, pageSize.value)

        while (result.items.length === 0 && result.total > 0 && page.value > FIRST_PAGE) {
          page.value -= 1
          result = await fetchRooms(page.value, pageSize.value)
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

    resetSelection()
    page.value = target
    await load()
  }

  function select(item: RoomItem): void {
    selectedId.value = item.id
  }

  function resetSelection(): void {
    selectedId.value = null
  }

  return {
    items,
    page,
    totalPages,
    isLoading,
    selectedId,
    selectedItem,
    load,
    changePage,
    select,
    resetSelection,
  }
}
