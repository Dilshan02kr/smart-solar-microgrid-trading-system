import type { AxiosResponse } from 'axios'
import type { CreateWebUserRequest, UpdateWebUserRequest, WebUser } from '@/features/users/types/userTypes'
import { apiClient } from '@/services/api/apiClient'
import { toApiClientError } from '@/services/api/apiError'

async function dataOrNormalizedError<T>(request: Promise<AxiosResponse<T>>): Promise<T> {
  try {
    return (await request).data
  } catch (error) {
    throw toApiClientError(error)
  }
}

export function getUsers(signal?: AbortSignal): Promise<WebUser[]> {
  return dataOrNormalizedError(apiClient.get<WebUser[]>('/api/users', { signal }))
}

export function getUser(userId: string, signal?: AbortSignal): Promise<WebUser> {
  return dataOrNormalizedError(apiClient.get<WebUser>(`/api/users/${encodeURIComponent(userId)}`, { signal }))
}

export function createUser(request: CreateWebUserRequest): Promise<WebUser> {
  return dataOrNormalizedError(apiClient.post<WebUser>('/api/users', request))
}

export function updateUser(userId: string, request: UpdateWebUserRequest): Promise<WebUser> {
  return dataOrNormalizedError(apiClient.put<WebUser>(`/api/users/${encodeURIComponent(userId)}`, request))
}

export function deactivateUser(userId: string): Promise<WebUser> {
  return dataOrNormalizedError(apiClient.patch<WebUser>(`/api/users/${encodeURIComponent(userId)}/deactivate`))
}
