import { useAuth } from '@/features/auth/hooks/useAuth'
import { UserRole } from '@/features/auth/types/authTypes'
import { OperatorDashboardPage } from '@/features/operator/pages/OperatorDashboardPage'
import { BackofficeDashboardPage } from '@/pages/BackofficeDashboardPage'

export function RoleAwareDashboardPage() {
  const { user } = useAuth()
  return user?.role === UserRole.GRID_OPERATOR ? <OperatorDashboardPage /> : <BackofficeDashboardPage />
}
