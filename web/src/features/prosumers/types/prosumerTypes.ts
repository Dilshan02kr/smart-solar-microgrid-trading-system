import type { AccountStatus } from '@/features/auth/types/authTypes'

export interface Prosumer {
  userId: string
  nic: string
  firstName: string
  lastName: string
  email: string
  phone: string
  accountStatus: AccountStatus
  createdAt: string
  updatedAt: string
}
