import { UserRole, type UserRole as UserRoleValue } from '@/features/auth/types/authTypes'

const roleLabels: Record<UserRoleValue, string> = {
  [UserRole.BACKOFFICE]: 'Backoffice',
  [UserRole.GRID_OPERATOR]: 'Grid Operator',
  [UserRole.PROSUMER]: 'Prosumer',
}

export function getRoleLabel(role: UserRoleValue): string {
  return roleLabels[role]
}
