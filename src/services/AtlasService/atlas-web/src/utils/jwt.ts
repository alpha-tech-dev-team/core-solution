import { Base64Url, Jwt, JwtClaim, TokenTime } from '@/constants/jwt.Constants'

const textDecoder = new TextDecoder()

export interface JwtPayload {
  exp?: number
  [claim: string]: unknown
}

function decodeBase64Url(value: string): string {
  const standard = value
    .replaceAll(Base64Url.Minus, Base64Url.Plus)
    .replaceAll(Base64Url.Underscore, Base64Url.Slash)

  const padded = standard.padEnd(
    Math.ceil(standard.length / Base64Url.BlockSize) * Base64Url.BlockSize,
    Base64Url.Padding,
  )

  const binary = atob(padded)
  return textDecoder.decode(Uint8Array.from(binary, (char) => char.charCodeAt(0)))
}

export function decodeJwtPayload(token: string): JwtPayload | null {
  const payload = token.split(Jwt.Separator)[Jwt.PayloadIndex]
  if (!payload) return null

  try {
    return JSON.parse(decodeBase64Url(payload)) as JwtPayload
  } catch {
    return null
  }
}

export function getTokenExpiryMs(token: string): number | null {
  const expiry = decodeJwtPayload(token)?.[JwtClaim.ExpiresAt]
  return typeof expiry === 'number' ? expiry * TokenTime.MsPerSecond : null
}

export function isExpired(expiresAtMs: number | null, skewMs = TokenTime.ClockSkewMs): boolean {
  if (expiresAtMs === null) return false
  return Date.now() >= expiresAtMs - skewMs
}
