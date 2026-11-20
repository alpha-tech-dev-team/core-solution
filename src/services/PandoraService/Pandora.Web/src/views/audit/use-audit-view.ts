import { onMounted, onUnmounted, ref } from 'vue'
import { FIRST_PAGE } from '@shared/constants/pagination'
import { fetchEmployeeNames, refreshEmployeeNames, type EmployeeNames } from '@/api/employees'
import { AUDIT_LIVE_REFRESH_DEBOUNCE_MS, AUDIT_MAX_NAME_MATCH_IDS } from '@/constants/audit'
import type { AuditEntry } from '@/models/audit-entry'
import { useAuditEntries } from './use-audit-entries'
import { useAuditFilters } from './use-audit-filters'
import { useAuditLiveUpdates } from './use-audit-live-updates'

export function useAuditView() {
  const employeeNames = ref<EmployeeNames>({ byId: new Map(), byTelegramId: new Map() })
  const tooManyMatches = ref(false)
  const filters = useAuditFilters(applyQuery)
  const entries = useAuditEntries(filters.query)
  let liveRefreshTimer: number | undefined

  const liveUpdates = useAuditLiveUpdates(addLiveEntry)

  async function applyQuery(): Promise<void> {
    const actors = matchActorIds(filters.query.search)
    tooManyMatches.value = actors.tooManyMatches
    filters.query.userIds = actors.userIds ?? ''
    filters.query.telegramUserIds = actors.telegramUserIds ?? ''
    entries.page.value = FIRST_PAGE

    if (tooManyMatches.value) {
      entries.entries.value = []
      return
    }

    await entries.load()
  }

  function matchActorIds(search: string | undefined): {
    userIds?: string
    telegramUserIds?: string
    tooManyMatches: boolean
  } {
    const needle = search?.trim().toLowerCase()

    if (!needle) return { tooManyMatches: false }

    const matchedEmployeeIds = new Set<string>()
    const userIds: string[] = []
    const telegramUserIds: string[] = []

    for (const [id, name] of employeeNames.value.byId) {
      if (!matchesName(name, needle)) continue
      matchedEmployeeIds.add(id)
      userIds.push(id)
    }

    for (const [telegramId, name] of employeeNames.value.byTelegramId) {
      if (matchesName(name, needle)) telegramUserIds.push(String(telegramId))
    }

    if (matchedEmployeeIds.size > AUDIT_MAX_NAME_MATCH_IDS) {
      return { tooManyMatches: true }
    }

    return {
      userIds: userIds.join(',') || undefined,
      telegramUserIds: telegramUserIds.join(',') || undefined,
      tooManyMatches: false,
    }
  }

  function matchesName(name: string, needle: string): boolean {
    const normalizedName = name.toLowerCase()

    return needle
      .split(' ')
      .filter(Boolean)
      .every((part) => normalizedName.includes(part))
  }

  function actorName(entry: AuditEntry): string {
    if (entry.telegramUserId !== null) {
      return (
        employeeNames.value.byTelegramId.get(entry.telegramUserId) ?? String(entry.telegramUserId)
      )
    }

    if (entry.userId !== null) {
      return employeeNames.value.byId.get(entry.userId) ?? entry.userId
    }

    return ''
  }

  function knowsActor(entry: AuditEntry): boolean {
    if (entry.telegramUserId !== null) {
      return employeeNames.value.byTelegramId.has(entry.telegramUserId)
    }

    if (entry.userId !== null) {
      return employeeNames.value.byId.has(entry.userId)
    }

    return true
  }

  async function loadEmployeeNames(): Promise<void> {
    try {
      employeeNames.value = await fetchEmployeeNames()
    } catch (error) {
      console.error('Не удалось загрузить сотрудников для журнала действий', error)
    }
  }

  async function refreshUnknownActor(entry: AuditEntry): Promise<void> {
    if (knowsActor(entry)) return

    try {
      employeeNames.value = await refreshEmployeeNames()
    } catch (error) {
      console.error('Не удалось обновить список сотрудников для журнала действий', error)
    }
  }

  function addLiveEntry(entry: AuditEntry): void {
    if (entries.entries.value.some((existing) => existing.id === entry.id)) return

    if (filters.isDefaultView.value) {
      entries.increaseTotal()

      if (entries.page.value === FIRST_PAGE) {
        entries.prepend(entry)
      } else {
        scheduleLiveRefresh()
      }
    } else {
      scheduleLiveRefresh()
    }

    void refreshUnknownActor(entry)
  }

  function scheduleLiveRefresh(): void {
    window.clearTimeout(liveRefreshTimer)
    liveRefreshTimer = window.setTimeout(() => void entries.load(), AUDIT_LIVE_REFRESH_DEBOUNCE_MS)
  }

  onMounted(() => {
    void entries.load()
    void loadEmployeeNames()
    void liveUpdates.start()
  })

  onUnmounted(() => {
    entries.dispose()
    filters.dispose()
    liveUpdates.dispose()
    window.clearTimeout(liveRefreshTimer)
  })

  return {
    query: filters.query,
    actionOptions: filters.actionOptions,
    sourceOptions: filters.sourceOptions,
    entries: entries.entries,
    page: entries.page,
    totalPages: entries.totalPages,
    isLoading: entries.isLoading,
    errorMessage: entries.errorMessage,
    tooManyMatches,
    changePage: entries.changePage,
    toggleSort: filters.toggleSort,
    isSortedBy: filters.isSortedBy,
    sortAriaValue: filters.sortAriaValue,
    sortIndicatorClass: filters.sortIndicatorClass,
    onSearchInput: filters.onSearchInput,
    applyFilters: applyQuery,
    auditActionLabel: filters.auditActionLabel,
    auditSourceLabel: filters.auditSourceLabel,
    actorName,
  }
}
