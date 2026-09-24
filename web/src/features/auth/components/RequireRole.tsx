import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@/features/auth/hooks/useAuth'
import type { UserRole } from '@/features/auth/types/authTypes'

export interface RequireRoleProps {
  allowedRoles: readonly UserRole[]
}

export function RequireRole({ allowedRoles }: RequireRoleProps) {
  const { user } = useAuth()
  if (!user || !allowedRoles.includes(user.role)) return <Navigate to="/403" replace />
  return <Outlet />
}
