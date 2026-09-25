import { useCallback, useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { getSlotsByStation } from '@/features/slots/api/slotsApi'
import { SlotsTable } from '@/features/slots/components/SlotsTable'
import type { EnergyBookingSlot } from '@/features/slots/types/slotTypes'
import { useStation } from '@/features/stations/hooks/useStation'
import { formatCapacity } from '@/features/stations/utils/stationPresentation'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

interface SlotsLocationState { success?: string }

export function StationSlotsPage() {
  const { stationId = '' } = useParams()
  const location = useLocation()
  const navigate = useNavigate()
  const { station, isLoading: stationLoading, notFound, error: stationError, retry: retryStation } = useStation(stationId)
  const [slots, setSlots] = useState<EnergyBookingSlot[]>([])
  const [resolvedStationId, setResolvedStationId] = useState<string | null>(null)
  const [isSlotsLoading, setIsSlotsLoading] = useState(true)
  const [slotsError, setSlotsError] = useState<ApiClientError | null>(null)
  const [successMessage] = useState((location.state as SlotsLocationState | null)?.success ?? '')
  const [requestVersion, setRequestVersion] = useState(0)

  const retry = useCallback(() => {
    setIsSlotsLoading(true)
    setSlotsError(null)
    setRequestVersion((version) => version + 1)
    retryStation()
  }, [retryStation])

  useEffect(() => {
    if ((location.state as SlotsLocationState | null)?.success) void navigate(location.pathname, { replace: true, state: null })
  }, [location.pathname, location.state, navigate])

  useEffect(() => {
    const controller = new AbortController()
    getSlotsByStation(stationId, controller.signal)
      .then((loadedSlots) => {
        setSlots(loadedSlots)
        setResolvedStationId(stationId)
        setSlotsError(null)
      })
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) {
          setResolvedStationId(stationId)
          setSlotsError(toApiClientError(requestError))
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsSlotsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion, stationId])

  const currentSlots = resolvedStationId === stationId ? slots : []
  const isLoading = stationLoading || isSlotsLoading || resolvedStationId !== stationId
  const error = stationError ?? (resolvedStationId === stationId ? slotsError : null)

  return (
    <>
      <PageHeader
        title={station ? `Energy Slots — ${station.name}` : 'Energy Slots'}
        description={station ? `${station.locationName} · ${formatCapacity(station.totalCapacityKw)} total capacity` : 'Manage booking slots for this microgrid node.'}
        actions={station ? <><Link className="button button--outline" to={`/stations/${station.id}`}>Node details</Link><Link className="button button--primary" to={`/stations/${station.id}/slots/new`}>Add Energy Slot</Link></> : undefined}
      />
      <PageContent>
        {successMessage && <Alert variant="success" title="Energy slots updated">{successMessage}</Alert>}
        {isLoading ? <div className="page-loading"><Spinner label="Loading energy slots" /></div>
          : notFound ? <EmptyState title="Microgrid node not found" description="The requested node could not be found." action={<Link className="button button--primary" to="/stations">Back to Microgrid Nodes</Link>} />
            : error ? <PageError message={error.message} onRetry={retry} />
              : currentSlots.length === 0 ? <EmptyState title="No energy slots found" description="Add an energy booking slot for this microgrid node." action={<Link className="button button--primary" to={`/stations/${stationId}/slots/new`}>Add Energy Slot</Link>} />
                : <SlotsTable slots={currentSlots} stationId={stationId} />}
      </PageContent>
    </>
  )
}
