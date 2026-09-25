import type { AxiosResponse } from 'axios'
import type { CreateStationRequest, Station, UpdateStationRequest } from '@/features/stations/types/stationTypes'
import { apiClient } from '@/services/api/apiClient'
import { toApiClientError } from '@/services/api/apiError'

async function dataOrNormalizedError<T>(request: Promise<AxiosResponse<T>>): Promise<T> {
  try {
    return (await request).data
  } catch (error) {
    throw toApiClientError(error)
  }
}

export function getStations(signal?: AbortSignal): Promise<Station[]> {
  return dataOrNormalizedError(apiClient.get<Station[]>('/api/stations', { signal }))
}

export function getStation(stationId: string, signal?: AbortSignal): Promise<Station> {
  return dataOrNormalizedError(apiClient.get<Station>(`/api/stations/${encodeURIComponent(stationId)}`, { signal }))
}

export function createStation(request: CreateStationRequest): Promise<Station> {
  return dataOrNormalizedError(apiClient.post<Station>('/api/stations', request))
}

export function updateStation(stationId: string, request: UpdateStationRequest): Promise<Station> {
  return dataOrNormalizedError(apiClient.put<Station>(`/api/stations/${encodeURIComponent(stationId)}`, request))
}

export function activateStation(stationId: string): Promise<Station> {
  return dataOrNormalizedError(apiClient.patch<Station>(`/api/stations/${encodeURIComponent(stationId)}/activate`))
}

export function deactivateStation(stationId: string): Promise<Station> {
  return dataOrNormalizedError(apiClient.patch<Station>(`/api/stations/${encodeURIComponent(stationId)}/deactivate`))
}
