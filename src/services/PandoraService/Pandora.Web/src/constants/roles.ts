export const PANDORA_ADMIN_ROLE = 'PandoraAdmin'

export const ADMIN_ROLES = ['AtlasAdmin', PANDORA_ADMIN_ROLE] as const

export function isAdminRole(role: string): boolean {
  return (ADMIN_ROLES as readonly string[]).includes(role)
}
