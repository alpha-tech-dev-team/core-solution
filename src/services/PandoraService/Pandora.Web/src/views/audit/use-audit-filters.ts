import { computed, reactive, type ComputedRef } from 'vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import {
  AUDIT_ACTION_KEYS,
  AUDIT_DEFAULT_SORT_BY,
  AUDIT_DEFAULT_SORT_DIRECTION,
  AUDIT_SEARCH_DEBOUNCE_MS,
  AUDIT_SORT_ARIA_VALUES,
  AUDIT_SORT_INDICATOR_CLASSES,
  AUDIT_SOURCE_KEYS,
  type AuditSortAriaValue,
} from '@/constants/audit'
import { API_SORT_DIRECTION, type AuditQuery } from '@/models/audit-entry'

export interface AuditFilters {
  query: AuditQuery
  actionOptions: string[]
  sourceOptions: string[]
  isDefaultView: ComputedRef<boolean>
  toggleSort(field: string): void
  isSortedBy(field: string): boolean
  sortAriaValue(field: string): AuditSortAriaValue
  sortIndicatorClass(field: string): string
  onSearchInput(): void
  auditActionLabel(action: string): string
  auditSourceLabel(source: string): string
  dispose(): void
}

export function useAuditFilters(onChange: () => void): AuditFilters {
  const { tr } = useTranslations()
  let searchTimer: number | undefined

  const query = reactive<AuditQuery>({
    search: '',
    action: '',
    requestSource: '',
    sortBy: AUDIT_DEFAULT_SORT_BY,
    sortDirection: AUDIT_DEFAULT_SORT_DIRECTION,
  })

  const isDefaultView = computed(
    () =>
      !query.search &&
      !query.action &&
      !query.requestSource &&
      query.sortBy === AUDIT_DEFAULT_SORT_BY &&
      query.sortDirection === AUDIT_DEFAULT_SORT_DIRECTION,
  )

  function isSortedBy(field: string): boolean {
    return query.sortBy === field
  }

  function sortAriaValue(field: string): AuditSortAriaValue {
    if (!isSortedBy(field)) return AUDIT_SORT_ARIA_VALUES.NONE

    return query.sortDirection === API_SORT_DIRECTION.ASCENDING
      ? AUDIT_SORT_ARIA_VALUES.ASCENDING
      : AUDIT_SORT_ARIA_VALUES.DESCENDING
  }

  function sortIndicatorClass(field: string): string {
    if (!isSortedBy(field)) return ''

    return query.sortDirection === API_SORT_DIRECTION.ASCENDING
      ? AUDIT_SORT_INDICATOR_CLASSES.ASCENDING
      : AUDIT_SORT_INDICATOR_CLASSES.DESCENDING
  }

  function toggleSort(field: string): void {
    if (query.sortBy === field) {
      query.sortDirection =
        query.sortDirection === API_SORT_DIRECTION.ASCENDING
          ? API_SORT_DIRECTION.DESCENDING
          : API_SORT_DIRECTION.ASCENDING
    } else {
      query.sortBy = field
      query.sortDirection = API_SORT_DIRECTION.ASCENDING
    }

    onChange()
  }

  function onSearchInput(): void {
    window.clearTimeout(searchTimer)
    searchTimer = window.setTimeout(onChange, AUDIT_SEARCH_DEBOUNCE_MS)
  }

  function auditActionLabel(action: string): string {
    const key = (AUDIT_ACTION_KEYS as Record<string, string>)[action]

    return key === undefined ? action : tr(key)
  }

  function auditSourceLabel(source: string): string {
    const key = (AUDIT_SOURCE_KEYS as Record<string, string>)[source]

    return key === undefined ? source : tr(key)
  }

  return {
    query,
    actionOptions: Object.keys(AUDIT_ACTION_KEYS),
    sourceOptions: Object.keys(AUDIT_SOURCE_KEYS),
    isDefaultView,
    toggleSort,
    isSortedBy,
    sortAriaValue,
    sortIndicatorClass,
    onSearchInput,
    auditActionLabel,
    auditSourceLabel,
    dispose: () => {
      window.clearTimeout(searchTimer)
    },
  }
}
