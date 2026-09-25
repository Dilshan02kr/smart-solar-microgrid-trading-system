import { Link } from 'react-router-dom'
import { DataTable } from '@/components/ui/DataTable'
import { ReservationStatusBadge } from '@/features/reservations/components/ReservationStatusBadge'
import type { Reservation } from '@/features/reservations/types/reservationTypes'
import { formatDateTime } from '@/utils/formatDate'

export interface ReservationsTableProps {
  reservations: Reservation[]
  prosumerNames: ReadonlyMap<string, string>
  stationNames: ReadonlyMap<string, string>
}

export function ReservationsTable({ reservations, prosumerNames, stationNames }: ReservationsTableProps) {
  return (
    <DataTable caption="Reservations">
      <thead>
        <tr><th scope="col">Reservation</th><th scope="col">Prosumer</th><th scope="col">Microgrid node</th><th scope="col">Scheduled time</th><th scope="col">Status</th><th scope="col">Created</th><th scope="col">Actions</th></tr>
      </thead>
      <tbody>
        {reservations.map((reservation) => (
          <tr key={reservation.reservationId}>
            <td><strong className="identifier-text">{reservation.reservationId}</strong></td>
            <td>{prosumerNames.get(reservation.prosumerId) ?? reservation.prosumerId}</td>
            <td>{stationNames.get(reservation.stationId) ?? reservation.stationId}</td>
            <td>{formatDateTime(reservation.scheduledTime)}</td>
            <td><ReservationStatusBadge status={reservation.status} /></td>
            <td>{formatDateTime(reservation.createdAt)}</td>
            <td><div className="table-actions"><Link className="button button--ghost" to={`/reservations/${reservation.reservationId}`}>View details</Link></div></td>
          </tr>
        ))}
      </tbody>
    </DataTable>
  )
}
