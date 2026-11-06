export const ADMIN_ROLES = ['AtlasAdmin', 'PandoraAdmin'] as const

export function isAdminRole(role: string): boolean {
  return (ADMIN_ROLES as readonly string[]).includes(role)
}
