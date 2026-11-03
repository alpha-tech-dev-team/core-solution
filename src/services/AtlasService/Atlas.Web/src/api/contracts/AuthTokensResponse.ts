export interface AuthTokensResponse {
  accessToken: string
  accessTokenExpiresAt?: string
  refreshToken: string
  refreshTokenExpiresAt?: string
}
