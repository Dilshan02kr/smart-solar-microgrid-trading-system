import { createContext } from 'react'
import type { AuthenticatedUser, WebLoginRequest } from '@/features/auth/types/authTypes'

export interface AuthContextValue {
  user: AuthenticatedUser | null
  isAuthenticated: boolean
  isInitializing: boolean
  login: (credentials: WebLoginRequest) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
