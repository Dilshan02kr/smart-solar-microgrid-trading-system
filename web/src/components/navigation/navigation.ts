import { UserRole, type UserRole as UserRoleValue } from '@/features/auth/types/authTypes'

export interface NavigationItem {
  label: string
  path: string
  shortLabel: string
  allowedRoles: readonly UserRoleValue[]
  available: boolean
}

export const navigationItems: NavigationItem[] = [
  { label: 'Dashboard', path: '/', shortLabel: 'DB', allowedRoles: [UserRole.BACKOFFICE, UserRole.GRID_OPERATOR], available: true },
  { label: 'Users', path: '/users', shortLabel: 'US', allowedRoles: [UserRole.BACKOFFICE], available: true },
  { label: 'Prosumers', path: '/prosumers', shortLabel: 'PR', allowedRoles: [UserRole.BACKOFFICE], available: true },
  { label: 'Microgrid Nodes', path: '/stations', shortLabel: 'MN', allowedRoles: [UserRole.BACKOFFICE], available: true },
  { label: 'Energy Slots', path: '/slots', shortLabel: 'ES', allowedRoles: [UserRole.BACKOFFICE], available: false },
  { label: 'Reservations', path: '/reservations', shortLabel: 'RS', allowedRoles: [UserRole.BACKOFFICE], available: true },
  { label: 'Operator Operations', path: '/operator', shortLabel: 'OP', allowedRoles: [UserRole.GRID_OPERATOR], available: false },
  { label: 'Components', path: '/components', shortLabel: 'UI', allowedRoles: [UserRole.BACKOFFICE, UserRole.GRID_OPERATOR], available: true },
]
