import { Navigate, Outlet } from 'react-router-dom'
import { AuthInitializing } from '@/features/auth/components/AuthInitializing'
import { useAuth } from '@/features/auth/hooks/useAuth'

export function PublicOnlyRoute() {
  const { isAuthenticated, isInitializing } = useAuth()

  if (isInitializing) return <AuthInitializing />
  if (isAuthenticated) return <Navigate to="/" replace />
  return <Outlet />
}
