import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from '@/features/auth/components/ProtectedRoute'
import { PublicOnlyRoute } from '@/features/auth/components/PublicOnlyRoute'
import { RequireRole } from '@/features/auth/components/RequireRole'
import { LoginPage } from '@/features/auth/pages/LoginPage'
import { WEB_APP_ROLES } from '@/features/auth/types/authTypes'
import { AppLayout } from '@/layouts/AppLayout'
import { AccessDeniedPage } from '@/pages/AccessDeniedPage'
import { ComponentShowcasePage } from '@/pages/ComponentShowcasePage'
import { DashboardPlaceholderPage } from '@/pages/DashboardPlaceholderPage'
import { NotFoundPage } from '@/pages/NotFoundPage'

export function AppRouter() {
  return (
    <Routes>
      <Route element={<PublicOnlyRoute />}>
        <Route path="/login" element={<LoginPage />} />
      </Route>
      <Route element={<ProtectedRoute />}>
        <Route path="/403" element={<AccessDeniedPage />} />
        <Route element={<RequireRole allowedRoles={WEB_APP_ROLES} />}>
          <Route element={<AppLayout />}>
            <Route index element={<DashboardPlaceholderPage />} />
            <Route path="components" element={<ComponentShowcasePage />} />
            <Route path="404" element={<NotFoundPage />} />
            <Route path="*" element={<Navigate to="/404" replace />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  )
}
