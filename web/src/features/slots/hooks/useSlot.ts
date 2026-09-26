import { useCallback, useEffect, useState } from 'react'
import { getSlot } from '@/features/slots/api/slotsApi'
import type { EnergyBookingSlot } from '@/features/slots/types/slotTypes'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function useSlot(slotId: string) {
  const [slot, setSlot] = useState<EnergyBookingSlot | null>(null)
  const [resolvedSlotId, setResolvedSlotId] = useState<string | null>(null)
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
    getSlot(slotId, controller.signal)
      .then((loadedSlot) => {
        setSlot(loadedSlot)
        setResolvedSlotId(slotId)
        setNotFound(false)
        setError(null)
      })
      .catch((requestError: unknown) => {
        if (controller.signal.aborted) return
        const normalized = toApiClientError(requestError)
        setSlot(null)
        setResolvedSlotId(slotId)
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
  }, [requestVersion, slotId])

  return {
    slot: resolvedSlotId === slotId ? slot : null,
    isLoading: isRequesting || resolvedSlotId !== slotId,
    notFound,
    error,
    retry,
  }
}
