import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { Button } from '@/components/ui/Button'
import { getProsumers } from '@/features/prosumers/api/prosumersApi'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { approveReservation, getReservation } from '@/features/reservations/api/reservationsApi'
import { ReservationSummary } from '@/features/reservations/components/ReservationSummary'
import { ReservationStatus, type Reservation } from '@/features/reservations/types/reservationTypes'
import { getStations } from '@/features/stations/api/stationsApi'
import type { Station } from '@/features/stations/types/stationTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function ReservationDetailsPage() {
  const { reservationId = '' } = useParams()
  const [reservation, setReservation] = useState<Reservation | null>(null)
  const [resolvedId, setResolvedId] = useState<string | null>(null)
  const [prosumers, setProsumers] = useState<Prosumer[]>([])
  const [stations, setStations] = useState<Station[]>([])
  const [lookupWarning, setLookupWarning] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [actionError, setActionError] = useState<ApiClientError | null>(null)
  const [successMessage, setSuccessMessage] = useState('')
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [isApproving, setIsApproving] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)

  function retry() {
    setIsLoading(true)
    setNotFound(false)
    setError(null)
    setRequestVersion((version) => version + 1)
  }

  useEffect(() => {
    const controller = new AbortController()
    Promise.allSettled([
      getReservation(reservationId, controller.signal),
      getProsumers(controller.signal),
      getStations(controller.signal),
    ]).then(([reservationResult, prosumerResult, stationResult]) => {
      if (controller.signal.aborted) return
      setResolvedId(reservationId)
      if (reservationResult.status === 'rejected') {
        const normalized = toApiClientError(reservationResult.reason)
        setReservation(null)
        if (normalized.status === 404) setNotFound(true)
        else setError(normalized)
        return
      }
      setReservation(reservationResult.value)
      setNotFound(false)
      setError(null)
      setProsumers(prosumerResult.status === 'fulfilled' ? prosumerResult.value : [])
      setStations(stationResult.status === 'fulfilled' ? stationResult.value : [])
      setLookupWarning(prosumerResult.status === 'rejected' || stationResult.status === 'rejected')
    }).finally(() => {
      if (!controller.signal.aborted) setIsLoading(false)
    })
    return () => controller.abort()
  }, [reservationId, requestVersion])

  const currentReservation = resolvedId === reservationId ? reservation : null
  const prosumer = prosumers.find((item) => item.userId === currentReservation?.prosumerId)
  const station = stations.find((item) => item.id === currentReservation?.stationId)

  async function handleApprove() {
    if (!currentReservation || isApproving) return
    setIsApproving(true)
    setActionError(null)
    try {
      const approved = await approveReservation(currentReservation.reservationId)
      setReservation(approved)
      setSuccessMessage('Reservation approved. The server-issued transaction reference is now available below.')
      setConfirmOpen(false)
    } catch (requestError) {
      const normalized = toApiClientError(requestError)
      setActionError(normalized)
      if (normalized.code === 'RESERVATION_NOT_FOUND') {
        setConfirmOpen(false)
        setReservation(null)
        setNotFound(true)
      } else if (normalized.code === 'INVALID_RESERVATION_STATE') {
        setConfirmOpen(false)
        setIsLoading(true)
        setRequestVersion((version) => version + 1)
      }
    } finally {
      setIsApproving(false)
    }
  }

  const actions = currentReservation ? <>
    <Link className="button button--outline" to="/reservations">Back to Reservations</Link>
    {currentReservation.status === ReservationStatus.PENDING && <Button onClick={() => { setActionError(null); setConfirmOpen(true) }}>Approve Reservation</Button>}
  </> : <Link className="button button--outline" to="/reservations">Back to Reservations</Link>

  return (
    <>
      <PageHeader title="Reservation details" description="Review reservation, participant, microgrid, and transaction information." actions={actions} />
      <PageContent>
        {successMessage && <Alert variant="success" title="Reservation approved">{successMessage}</Alert>}
        {actionError && <Alert variant="error" title="Unable to approve reservation">{actionError.message}</Alert>}
        {lookupWarning && currentReservation && <Alert variant="warning" title="Some names are unavailable">The reservation is shown with ID fallbacks because related details could not be loaded.</Alert>}
        {isLoading || resolvedId !== reservationId ? (
          <div className="page-loading"><Spinner label="Loading reservation details" /></div>
        ) : notFound ? (
          <EmptyState title="Reservation not found" description="The requested reservation could not be found." action={<Link className="button button--primary" to="/reservations">Back to Reservations</Link>} />
        ) : error ? (
          <PageError message={error.message} onRetry={retry} />
        ) : currentReservation ? (
          <ReservationSummary reservation={currentReservation} prosumerName={prosumer ? `${prosumer.firstName} ${prosumer.lastName}` : undefined} prosumerNic={prosumer?.nic} stationName={station?.name} />
        ) : null}
      </PageContent>
      <ConfirmationDialog
        open={confirmOpen}
        title="Approve reservation?"
        description="Approve this reservation? Approval will issue the transaction reference used later by the Prosumer QR workflow."
        confirmLabel="Approve Reservation"
        confirmVariant="primary"
        errorMessage={actionError?.message}
        loading={isApproving}
        onCancel={() => { setConfirmOpen(false); setActionError(null) }}
        onConfirm={() => void handleApprove()}
      />
    </>
  )
}
