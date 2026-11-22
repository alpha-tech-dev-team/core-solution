import { onMounted, ref } from 'vue'
import { useViewError } from '@shared/composables/use-view-error'
import { INCIDENTS_PAGE_SIZE } from '@/constants/pagination'
import { useIncidentsList } from './use-incidents-list'

export function useIncidentsView() {
  const { errorMessage, run } = useViewError()

  const pageSize = ref(INCIDENTS_PAGE_SIZE)
  const total = ref(0)

  const list = useIncidentsList(pageSize, total, run)

  onMounted(list.load)

  return {
    incidents: list.items,
    page: list.page,
    totalPages: list.totalPages,
    isLoading: list.isLoading,
    errorMessage,
    changePage: list.changePage,
  }
}
