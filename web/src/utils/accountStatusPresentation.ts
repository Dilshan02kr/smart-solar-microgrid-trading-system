import type { BadgeVariant } from '@/components/ui/Badge'
import { AccountStatus, type AccountStatus as AccountStatusValue } from '@/features/auth/types/authTypes'

const badgeVariants: Record<AccountStatusValue, BadgeVariant> = {
  [AccountStatus.PENDING]: 'warning',
  [AccountStatus.ACTIVE]: 'success',
  [AccountStatus.DEACTIVATED]: 'danger',
}

export function getAccountStatusBadgeVariant(status: AccountStatusValue): BadgeVariant {
  return badgeVariants[status]
}
