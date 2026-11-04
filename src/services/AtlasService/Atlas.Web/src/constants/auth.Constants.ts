export const StorageKey = {
  AccessExpiresAt: 'access_token_expires_at',
} as const

export const StoreId = {
  Auth: 'auth',
} as const

export const TokenRefresh = {
  MarginMs: 60_000,
  MinDelayMs: 5_000,
  ClockTickMs: 30_000,
} as const
