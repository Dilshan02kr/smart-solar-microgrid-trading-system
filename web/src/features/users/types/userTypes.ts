import { AccountStatus, UserRole } from '@/features/auth/types/authTypes'

export type WebUserRole = typeof UserRole.BACKOFFICE | typeof UserRole.GRID_OPERATOR
export type WebUserAccountStatus = typeof AccountStatus.ACTIVE | typeof AccountStatus.DEACTIVATED

export interface WebUser {
  userId: string
  firstName: string
  lastName: string
  email: string
  phone: string
  role: WebUserRole
  accountStatus: WebUserAccountStatus
  assignedMicrogridNodeId: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateWebUserRequest {
  firstName: string
  lastName: string
  email: string
  phone: string
  password: string
  confirmPassword: string
  role: WebUserRole
  assignedMicrogridNodeId?: string
}

export interface UpdateWebUserRequest {
  firstName: string
  lastName: string
  email: string
  phone: string
  assignedMicrogridNodeId?: string
}

export interface StationResponse {
  id: string
  name: string
  locationName: string
  latitude: number
  longitude: number
  totalCapacityKw: number
  operationalSchedule: string
  status: string
  createdAt: string
  updatedAt: string
}
