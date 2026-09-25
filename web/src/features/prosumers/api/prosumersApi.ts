import type { AxiosResponse } from 'axios'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { apiClient } from '@/services/api/apiClient'
import { toApiClientError } from '@/services/api/apiError'

async function dataOrNormalizedError<T>(request: Promise<AxiosResponse<T>>): Promise<T> {
  try {
    return (await request).data
  } catch (error) {
    throw toApiClientError(error)
  }
}

export function getProsumers(signal?: AbortSignal): Promise<Prosumer[]> {
  return dataOrNormalizedError(apiClient.get<Prosumer[]>('/api/prosumers', { signal }))
}

export function getPendingProsumers(signal?: AbortSignal): Promise<Prosumer[]> {
  return dataOrNormalizedError(apiClient.get<Prosumer[]>('/api/prosumers/pending', { signal }))
}

export function getProsumer(prosumerId: string, signal?: AbortSignal): Promise<Prosumer> {
  return dataOrNormalizedError(apiClient.get<Prosumer>(`/api/prosumers/${encodeURIComponent(prosumerId)}`, { signal }))
}

export function activateProsumer(prosumerId: string): Promise<Prosumer> {
  return dataOrNormalizedError(apiClient.patch<Prosumer>(`/api/prosumers/${encodeURIComponent(prosumerId)}/activate`))
}

export function reactivateProsumer(prosumerId: string): Promise<Prosumer> {
  return dataOrNormalizedError(apiClient.patch<Prosumer>(`/api/prosumers/${encodeURIComponent(prosumerId)}/reactivate`))
}
