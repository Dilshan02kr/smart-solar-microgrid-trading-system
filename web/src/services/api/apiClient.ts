import axios from 'axios'
import { authSession } from '@/features/auth/session/authSession'
import { apiBaseUrl } from '@/services/api/apiConfig'
import type { InternalApiRequestConfig } from '@/services/api/apiTypes'

type UnauthorizedHandler = () => void

let unauthorizedHandler: UnauthorizedHandler | null = null

export function setUnauthorizedHandler(handler: UnauthorizedHandler | null): void {
  unauthorizedHandler = handler
}

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 10_000,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use((config) => {
  const requestConfig = config as InternalApiRequestConfig
  const token = requestConfig.skipAuthentication ? null : authSession.getToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      const config = error.config as InternalApiRequestConfig | undefined
      if (!config?.skipUnauthorizedHandling) {
        authSession.clearToken()
        unauthorizedHandler?.()
      }
    }

    return Promise.reject(error)
  },
)
