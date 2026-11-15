export const STORAGE_KEY = {
  ACCESS_EXPIRES_AT: 'access_token_expires_at',
} as const

export const STORE_ID = {
  AUTH: 'auth',
} as const

export const TOKEN_REFRESH = {
  MARGIN_MS: 60_000,
  MIN_DELAY_MS: 5_000,
  CLOCK_TICK_MS: 30_000,
} as const
