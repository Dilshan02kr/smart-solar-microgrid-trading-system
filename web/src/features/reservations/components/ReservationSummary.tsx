import { Link } from 'react-router-dom'
import { Card } from '@/components/ui/Card'
import { ReservationStatusBadge } from '@/features/reservations/components/ReservationStatusBadge'
import type { Reservation } from '@/features/reservations/types/reservationTypes'
import { formatDateTime } from '@/utils/formatDate'

export interface ReservationSummaryProps {
  reservation: Reservation
  prosumerName?: string
  prosumerNic?: string
  stationName?: string
}

export function ReservationSummary({ reservation, prosumerName, prosumerNic, stationName }: ReservationSummaryProps) {
  return (
    <div className="reservation-summary">
      <Card title="Reservation">
        <dl className="details-grid reservation-summary__grid">
          <div><dt>Reservation ID</dt><dd>{reservation.reservationId}</dd></div>
          <div><dt>Status</dt><dd><ReservationStatusBadge status={reservation.status} /></dd></div>
          <div><dt>Scheduled time</dt><dd>{formatDateTime(reservation.scheduledTime)}</dd></div>
          <div><dt>Created</dt><dd>{formatDateTime(reservation.createdAt)}</dd></div>
          <div><dt>Last updated</dt><dd>{formatDateTime(reservation.updatedAt)}</dd></div>
          <div><dt>Completed</dt><dd>{formatDateTime(reservation.completedAt)}</dd></div>
        </dl>
      </Card>
      <Card title="Prosumer">
        <dl className="details-grid reservation-summary__grid">
          <div><dt>Name</dt><dd>{prosumerName ?? 'Name unavailable'}</dd></div>
          <div><dt>NIC</dt><dd>{prosumerNic ?? 'NIC unavailable'}</dd></div>
          <div><dt>Prosumer ID</dt><dd>{reservation.prosumerId}</dd></div>
        </dl>
        <div className="reservation-summary__actions">
          <Link to={`/prosumers/${reservation.prosumerId}`}>View Prosumer</Link>
          <Link to={`/reservations?prosumerId=${encodeURIComponent(reservation.prosumerId)}`}>View this Prosumer's reservations</Link>
        </div>
      </Card>
      <Card title="Microgrid">
        <dl className="details-grid reservation-summary__grid">
          <div><dt>Node</dt><dd>{stationName ?? 'Node name unavailable'}</dd></div>
          <div><dt>Station ID</dt><dd>{reservation.stationId}</dd></div>
          <div><dt>Slot ID</dt><dd>{reservation.slotId}</dd></div>
        </dl>
        <div className="reservation-summary__actions"><Link to={`/stations/${reservation.stationId}`}>View Station</Link></div>
      </Card>
      <Card title="Transaction">
        <dl className="details-grid reservation-summary__grid">
          <div><dt>Transaction reference</dt><dd className="identifier-text">{reservation.transactionReference ?? 'Not issued'}</dd></div>
        </dl>
      </Card>
    </div>
  )
}
