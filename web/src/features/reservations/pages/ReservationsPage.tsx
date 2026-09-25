import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { getProsumers } from '@/features/prosumers/api/prosumersApi'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { ReservationFilters } from '@/features/reservations/components/ReservationFilters'
import { ReservationsTable } from '@/features/reservations/components/ReservationsTable'
import { searchReservations } from '@/features/reservations/api/reservationsApi'
import type { Reservation, ReservationSearchParams } from '@/features/reservations/types/reservationTypes'
import { getStations } from '@/features/stations/api/stationsApi'
import type { Station } from '@/features/stations/types/stationTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

function initialFilters(prosumerId: string | null): ReservationSearchParams {
  return prosumerId ? { prosumerId } : {}
}

export function ReservationsPage() {
  const [searchParams] = useSearchParams()
  const [filters, setFilters] = useState<ReservationSearchParams>(() => initialFilters(searchParams.get('prosumerId')))
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [prosumers, setProsumers] = useState<Prosumer[]>([])
  const [stations, setStations] = useState<Station[]>([])
  const [lookupWarning, setLookupWarning] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [requestVersion, setRequestVersion] = useState(0)

  function retry() {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }

  function applyFilters(nextFilters: ReservationSearchParams) {
    setIsLoading(true)
    setError(null)
    setFilters(nextFilters)
  }

  useEffect(() => {
    const controller = new AbortController()
    Promise.allSettled([
      searchReservations(filters, controller.signal),
      getProsumers(controller.signal),
      getStations(controller.signal),
    ]).then(([reservationResult, prosumerResult, stationResult]) => {
      if (controller.signal.aborted) return
      if (reservationResult.status === 'rejected') {
        setError(toApiClientError(reservationResult.reason))
        return
      }
      setReservations(reservationResult.value)
      setProsumers(prosumerResult.status === 'fulfilled' ? prosumerResult.value : [])
      setStations(stationResult.status === 'fulfilled' ? stationResult.value : [])
      setLookupWarning(prosumerResult.status === 'rejected' || stationResult.status === 'rejected')
    }).finally(() => {
      if (!controller.signal.aborted) setIsLoading(false)
    })
    return () => controller.abort()
  }, [filters, requestVersion])

  const prosumerNames = new Map(prosumers.map((prosumer) => [prosumer.userId, `${prosumer.firstName} ${prosumer.lastName} (${prosumer.nic})`]))
  const stationNames = new Map(stations.map((station) => [station.id, station.name]))
  const isFiltered = Boolean(filters.prosumerId || filters.status || filters.searchTerm)

  return (
    <>
      <PageHeader title="Reservations" description="Monitor reservation activity, search records, and review pending approvals." />
      <PageContent>
        <ReservationFilters key={JSON.stringify(filters)} filters={filters} prosumers={prosumers} onApply={applyFilters} />
        {lookupWarning && !error && <Alert variant="warning" title="Some names are unavailable">Reservations are shown with ID fallbacks because a related-data lookup could not be loaded.</Alert>}
        {isLoading ? (
          <div className="page-loading"><Spinner label="Loading reservations" /></div>
        ) : error ? (
          <PageError message={error.message} onRetry={retry} />
        ) : reservations.length === 0 ? (
          <EmptyState title={isFiltered ? 'No matching reservations' : 'No reservations found'} description={isFiltered ? 'No reservations match the selected filters.' : 'Reservations will appear here when Prosumers create them.'} />
        ) : (
          <ReservationsTable reservations={reservations} prosumerNames={prosumerNames} stationNames={stationNames} />
        )}
      </PageContent>
    </>
  )
}
