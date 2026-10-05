import { useEffect, useMemo, useState } from 'react'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { Button } from '@/components/ui/Button'
import { DataTable } from '@/components/ui/DataTable'
import { Select } from '@/components/ui/Select'
import { getOperatorReservations } from '@/features/operator/api/operatorApi'
import type { OperatorReservation } from '@/features/operator/types/operatorTypes'
import { getOperatorErrorMessage } from '@/features/operator/utils/operatorPresentation'
import { ReservationStatusBadge } from '@/features/reservations/components/ReservationStatusBadge'
import { ReservationStatus } from '@/features/reservations/types/reservationTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'
import { formatDateTime } from '@/utils/formatDate'

type OperationalStatus = 'ALL' | typeof ReservationStatus.PENDING | typeof ReservationStatus.APPROVED

export function OperatorReservationsPage() {
  const [reservations, setReservations] = useState<OperatorReservation[]>([])
  const [status, setStatus] = useState<OperationalStatus>('ALL')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ApiClientError | null>(null)
  const [requestVersion, setRequestVersion] = useState(0)

  useEffect(() => {
    const controller = new AbortController()
    getOperatorReservations(controller.signal)
      .then((response) => { setReservations(response); setError(null) })
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) setError(toApiClientError(requestError))
      })
      .finally(() => { if (!controller.signal.aborted) setIsLoading(false) })
    return () => controller.abort()
  }, [requestVersion])

  const visibleReservations = useMemo(
    () => reservations.filter((reservation) => status === 'ALL' || reservation.status === status),
    [reservations, status],
  )

  function retry() {
    setIsLoading(true)
    setError(null)
    setRequestVersion((version) => version + 1)
  }

  return (
    <>
      <PageHeader title="Operational Reservations" description="Monitor pending and approved reservations scoped by the server to your assigned microgrid node." actions={<Button variant="outline" onClick={retry} disabled={isLoading}>Refresh</Button>} />
      <PageContent>
        <div className="operator-reservation-filter">
          <Select label="Status" value={status} onChange={(event) => setStatus(event.target.value as OperationalStatus)} options={[
            { value: 'ALL', label: 'All' },
            { value: ReservationStatus.PENDING, label: 'Pending' },
            { value: ReservationStatus.APPROVED, label: 'Approved' },
          ]} />
        </div>
        {isLoading ? (
          <div className="page-loading"><Spinner label="Loading operational reservations" /></div>
        ) : error ? (
          <PageError message={getOperatorErrorMessage(error)} onRetry={retry} />
        ) : visibleReservations.length === 0 ? (
          <EmptyState title="No operational reservations" description="No pending or approved reservations match this status." />
        ) : (
          <DataTable caption="Assigned microgrid node operational reservations">
            <thead><tr><th scope="col">Scheduled time</th><th scope="col">Status</th><th scope="col">Reservation</th><th scope="col">Slot</th></tr></thead>
            <tbody>{visibleReservations.map((reservation) => (
              <tr key={reservation.reservationId}>
                <td>{formatDateTime(reservation.scheduledTime)}</td>
                <td><ReservationStatusBadge status={reservation.status} /></td>
                <td><span className="identifier-text">{reservation.reservationId}</span></td>
                <td><span className="identifier-text">{reservation.slotId}</span></td>
              </tr>
            ))}</tbody>
          </DataTable>
        )}
      </PageContent>
    </>
  )
}
