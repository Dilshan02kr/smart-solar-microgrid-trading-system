import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { ReservationStatusBadge } from '@/features/reservations/components/ReservationStatusBadge'
import { ReservationStatus } from '@/features/reservations/types/reservationTypes'
import type { OperatorTransaction } from '@/features/operator/types/operatorTypes'
import { formatDateTime } from '@/utils/formatDate'

export interface VerifiedTransactionCardProps {
  transaction: OperatorTransaction
  completionUnavailable?: boolean
  onComplete: () => void
  onReset: () => void
}

export function VerifiedTransactionCard({ transaction, completionUnavailable = false, onComplete, onReset }: VerifiedTransactionCardProps) {
  const canComplete = transaction.status === ReservationStatus.APPROVED && !completionUnavailable

  return (
    <Card className="verified-transaction" title={transaction.status === ReservationStatus.COMPLETED ? 'Completed energy transfer' : 'Verified transaction'}>
      <div className="verified-transaction__status">
        <p>{transaction.status === ReservationStatus.COMPLETED ? 'The energy-transfer reservation is complete.' : 'The transaction reference is valid for your assigned microgrid node.'}</p>
        <ReservationStatusBadge status={transaction.status} />
      </div>
      <dl className="details-grid verified-transaction__details">
        <div><dt>Reservation ID</dt><dd>{transaction.reservationId}</dd></div>
        <div><dt>Transaction reference</dt><dd>{transaction.transactionReference}</dd></div>
        <div><dt>Prosumer ID</dt><dd>{transaction.prosumerId}</dd></div>
        <div><dt>Station ID</dt><dd>{transaction.stationId}</dd></div>
        <div><dt>Slot ID</dt><dd>{transaction.slotId}</dd></div>
        <div><dt>Scheduled time</dt><dd>{formatDateTime(transaction.scheduledTime)}</dd></div>
      </dl>
      <div className="verified-transaction__actions">
        <Button variant="outline" onClick={onReset}>Verify Another Transaction</Button>
        {canComplete && <Button onClick={onComplete}>Complete Energy Transfer</Button>}
      </div>
    </Card>
  )
}
