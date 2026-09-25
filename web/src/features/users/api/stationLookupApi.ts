import type { AxiosResponse } from 'axios'
import type { StationResponse } from '@/features/users/types/userTypes'
import { apiClient } from '@/services/api/apiClient'
import { toApiClientError } from '@/services/api/apiError'

export async function getStationLookup(signal?: AbortSignal): Promise<StationResponse[]> {
  try {
    const response: AxiosResponse<StationResponse[]> = await apiClient.get('/api/stations', { signal })
    return response.data
  } catch (error) {
    throw toApiClientError(error)
  }
}
