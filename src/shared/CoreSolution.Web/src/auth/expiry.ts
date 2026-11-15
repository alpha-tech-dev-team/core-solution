export function toEpochMs(value: string | number | null | undefined): number | null {
  if (value === null || value === undefined || value === '') return null

  if (typeof value === 'number') return Number.isFinite(value) ? value : null

  const numeric = Number(value)
  if (!Number.isNaN(numeric)) return numeric

  const parsed = Date.parse(value)
  return Number.isNaN(parsed) ? null : parsed
}

export function isExpired(expiresAtMs: number | null, nowMs: number): boolean {
  if (expiresAtMs === null) return true

  return nowMs >= expiresAtMs
}
