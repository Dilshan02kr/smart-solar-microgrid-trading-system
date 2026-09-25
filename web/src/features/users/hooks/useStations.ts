import { useCallback, useEffect, useState } from 'react'
import { getStationLookup } from '@/features/users/api/stationLookupApi'
import type { StationResponse } from '@/features/users/types/userTypes'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function useStations() {
  const [stations, setStations] = useState<StationResponse[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [requestVersion, setRequestVersion] = useState(0)

  const retry = useCallback(() => {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    getStationLookup(controller.signal)
      .then(setStations)
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) setError(toApiClientError(requestError))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion])

  return { stations, isLoading, error, retry }
}
