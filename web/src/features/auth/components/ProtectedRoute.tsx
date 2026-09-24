import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { AuthInitializing } from '@/features/auth/components/AuthInitializing'
import { useAuth } from '@/features/auth/hooks/useAuth'

export function ProtectedRoute() {
  const { isAuthenticated, isInitializing } = useAuth()
  const location = useLocation()

  if (isInitializing) return <AuthInitializing />
  if (!isAuthenticated) return <Navigate to="/login" replace state={{ from: location.pathname }} />
  return <Outlet />
}
