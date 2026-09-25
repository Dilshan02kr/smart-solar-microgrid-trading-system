import { Navigate, Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute";
import { PublicOnlyRoute } from "@/features/auth/components/PublicOnlyRoute";
import { RequireRole } from "@/features/auth/components/RequireRole";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { UserRole, WEB_APP_ROLES } from "@/features/auth/types/authTypes";
import { PendingProsumersPage } from "@/features/prosumers/pages/PendingProsumersPage";
import { ProsumerDetailsPage } from "@/features/prosumers/pages/ProsumerDetailsPage";
import { ProsumersPage } from "@/features/prosumers/pages/ProsumersPage";
import { CreateUserPage } from "@/features/users/pages/CreateUserPage";
import { EditUserPage } from "@/features/users/pages/EditUserPage";
import { UsersPage } from "@/features/users/pages/UsersPage";
import { AppLayout } from "@/layouts/AppLayout";
import { AccessDeniedPage } from "@/pages/AccessDeniedPage";
import { ComponentShowcasePage } from "@/pages/ComponentShowcasePage";
import { DashboardPlaceholderPage } from "@/pages/DashboardPlaceholderPage";
import { NotFoundPage } from "@/pages/NotFoundPage";

const BACKOFFICE_ROLES = [UserRole.BACKOFFICE] as const;

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
            <Route element={<RequireRole allowedRoles={BACKOFFICE_ROLES} />}>
              <Route path="users" element={<UsersPage />} />
              <Route path="users/new" element={<CreateUserPage />} />
              <Route path="users/:userId/edit" element={<EditUserPage />} />
              <Route path="prosumers" element={<ProsumersPage />} />
              <Route
                path="prosumers/pending"
                element={<PendingProsumersPage />}
              />
              <Route
                path="prosumers/:prosumerId"
                element={<ProsumerDetailsPage />}
              />
            </Route>
            <Route path="404" element={<NotFoundPage />} />
            <Route path="*" element={<Navigate to="/404" replace />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  );
}
