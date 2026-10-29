export interface AuthTokens {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
}

export interface StoredSession {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
}