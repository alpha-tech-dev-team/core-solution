export const SECRET_SOURCE = {
  DATABASE: 'Database',
  VAULTWARDEN: 'Vaultwarden',
} as const

export type SecretSource = (typeof SECRET_SOURCE)[keyof typeof SECRET_SOURCE]
