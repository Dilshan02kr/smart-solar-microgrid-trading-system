import { useAuth } from '@/features/auth/hooks/useAuth'
import { UserRole } from '@/features/auth/types/authTypes'
import { OperatorDashboardPage } from '@/features/operator/pages/OperatorDashboardPage'
import { DashboardPlaceholderPage } from '@/pages/DashboardPlaceholderPage'

export function RoleAwareDashboardPage() {
  const { user } = useAuth()
  return user?.role === UserRole.GRID_OPERATOR ? <OperatorDashboardPage /> : <DashboardPlaceholderPage />
}
