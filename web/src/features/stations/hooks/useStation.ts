import { useCallback, useEffect, useState } from 'react'
import { getStation } from '@/features/stations/api/stationsApi'
import type { Station } from '@/features/stations/types/stationTypes'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function useStation(stationId: string) {
  const [station, setStation] = useState<Station | null>(null)
  const [resolvedStationId, setResolvedStationId] = useState<string | null>(null)
  const [isRequesting, setIsRequesting] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [requestVersion, setRequestVersion] = useState(0)

  const retry = useCallback(() => {
    setIsRequesting(true)
    setNotFound(false)
    setError(null)
    setRequestVersion((version) => version + 1)
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    getStation(stationId, controller.signal)
      .then((loadedStation) => {
        setStation(loadedStation)
        setResolvedStationId(stationId)
        setNotFound(false)
        setError(null)
      })
      .catch((requestError: unknown) => {
        if (controller.signal.aborted) return
        const normalized = toApiClientError(requestError)
        setStation(null)
        setResolvedStationId(stationId)
        if (normalized.status === 404) {
          setNotFound(true)
          setError(null)
        } else {
          setNotFound(false)
          setError(normalized)
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsRequesting(false)
      })
    return () => controller.abort()
  }, [requestVersion, stationId])

  const currentStation = resolvedStationId === stationId ? station : null
  return {
    station: currentStation,
    setStation,
    isLoading: isRequesting || resolvedStationId !== stationId,
    notFound,
    error,
    retry,
  }
}
