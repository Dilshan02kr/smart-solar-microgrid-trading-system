import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { getCurrentUser, webLogin } from '@/features/auth/api/authApi'
import { AuthContext, type AuthContextValue } from '@/features/auth/context/AuthContext'
import { authSession } from '@/features/auth/session/authSession'
import { AccountStatus, isWebAppRole, type AuthenticatedUser, type WebLoginRequest } from '@/features/auth/types/authTypes'
import { ApiClientError } from '@/services/api/apiError'
import { setUnauthorizedHandler } from '@/services/api/apiClient'

export interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthenticatedUser | null>(null)
  const [isInitializing, setIsInitializing] = useState(true)

  const logout = useCallback(() => {
    authSession.clearToken()
    setUser(null)
    setIsInitializing(false)
  }, [])

  useEffect(() => {
    setUnauthorizedHandler(logout)
    return () => setUnauthorizedHandler(null)
  }, [logout])

  useEffect(() => {
    const controller = new AbortController()

    async function restoreAuthentication() {
      if (!authSession.getToken()) {
        setIsInitializing(false)
        return
      }

      try {
        const restoredUser = await getCurrentUser(controller.signal)
        if (!isWebAppRole(restoredUser.role) || restoredUser.accountStatus !== AccountStatus.ACTIVE) {
          authSession.clearToken()
          if (!controller.signal.aborted) setUser(null)
          return
        }

        if (!controller.signal.aborted) setUser(restoredUser)
      } catch {
        if (!controller.signal.aborted) setUser(null)
      } finally {
        if (!controller.signal.aborted) setIsInitializing(false)
      }
    }

    void restoreAuthentication()
    return () => controller.abort()
  }, [])

  const login = useCallback(async (credentials: WebLoginRequest) => {
    authSession.clearToken()
    const response = await webLogin(credentials)
    if (!isWebAppRole(response.user.role)) {
      authSession.clearToken()
      throw new ApiClientError('This account is not permitted to use the Web application.', 'ACCESS_DENIED', 403)
    }
    if (response.user.accountStatus !== AccountStatus.ACTIVE) {
      authSession.clearToken()
      throw new ApiClientError('This account is not active.', 'ACCOUNT_NOT_ACTIVE', 403)
    }

    if (!authSession.setToken(response.token)) {
      throw new ApiClientError('A secure browser session could not be created. Check browser storage settings and try again.', 'SESSION_STORAGE_UNAVAILABLE')
    }
    setUser(response.user)
  }, [])

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isAuthenticated: user !== null,
    isInitializing,
    login,
    logout,
  }), [isInitializing, login, logout, user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
