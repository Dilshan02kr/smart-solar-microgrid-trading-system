import { apiClient } from '@/services/api/apiClient'
import type { AuthenticatedUser, AuthenticationResponse, WebLoginRequest } from '@/features/auth/types/authTypes'
import type { ApiRequestConfig } from '@/services/api/apiTypes'

export async function webLogin(credentials: WebLoginRequest): Promise<AuthenticationResponse> {
  const config: ApiRequestConfig = {
    skipAuthentication: true,
    skipUnauthorizedHandling: true,
  }
  const response = await apiClient.post<AuthenticationResponse>('/api/auth/web-login', credentials, config)
  return response.data
}

export async function getCurrentUser(signal?: AbortSignal): Promise<AuthenticatedUser> {
  const response = await apiClient.get<AuthenticatedUser>('/api/auth/me', { signal })
  return response.data
}
