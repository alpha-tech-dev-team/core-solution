import { TokenTime } from '@/constants/jwt.Constants'

export function isExpired(
  expiresAtMs: number | null,
  nowMs: number,
  skewMs = TokenTime.ClockSkewMs,
): boolean {
  if (expiresAtMs === null) return true
  return nowMs >= expiresAtMs - skewMs
}
