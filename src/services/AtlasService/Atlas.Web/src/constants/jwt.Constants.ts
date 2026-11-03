export const Jwt = {
  Separator: '.',
  PayloadIndex: 1,
} as const

export const JwtClaim = {
  ExpiresAt: 'exp',
} as const

export const Base64Url = {
  Padding: '=',
  Plus: '+',
  Minus: '-',
  Slash: '/',
  Underscore: '_',
  BlockSize: 4,
} as const

export const TokenTime = {
  MsPerSecond: 1000,
  ClockSkewMs: 0,
} as const
