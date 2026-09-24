export const UserRole = {
  BACKOFFICE: 'BACKOFFICE',
  GRID_OPERATOR: 'GRID_OPERATOR',
  PROSUMER: 'PROSUMER',
} as const

export type UserRole = (typeof UserRole)[keyof typeof UserRole]

export const AccountStatus = {
  PENDING: 'PENDING',
  ACTIVE: 'ACTIVE',
  DEACTIVATED: 'DEACTIVATED',
} as const

export type AccountStatus = (typeof AccountStatus)[keyof typeof AccountStatus]

export interface AuthenticatedUser {
  userId: string
  firstName: string
  lastName: string
  role: UserRole
  accountStatus: AccountStatus
  nic: string | null
  assignedMicrogridNodeId: string | null
}

export interface AuthenticationResponse {
  token: string
  expiresAtUtc: string
  user: AuthenticatedUser
}

export interface WebLoginRequest {
  email: string
  password: string
}

export const WEB_APP_ROLES = [UserRole.BACKOFFICE, UserRole.GRID_OPERATOR] as const

export function isWebAppRole(role: UserRole): role is typeof WEB_APP_ROLES[number] {
  return WEB_APP_ROLES.includes(role as typeof WEB_APP_ROLES[number])
}
