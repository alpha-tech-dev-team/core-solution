export const SESSION_STORAGE_KEY = 'auth_tokens'

export const SESSION_FIELDS = {
  accessToken: 'accessToken',
  refreshToken: 'refreshToken',
  accessTokenExpiresAt: 'accessTokenExpiresAt'
} as const