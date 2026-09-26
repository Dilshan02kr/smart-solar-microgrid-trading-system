import { Badge } from '@/components/ui/Badge'
import type { ReservationStatus } from '@/features/reservations/types/reservationTypes'
import { getReservationStatusBadgeVariant, getReservationStatusLabel } from '@/features/reservations/utils/reservationPresentation'

export interface ReservationStatusBadgeProps {
  status: ReservationStatus
}

export function ReservationStatusBadge({ status }: ReservationStatusBadgeProps) {
  return <Badge variant={getReservationStatusBadgeVariant(status)}>{getReservationStatusLabel(status)}</Badge>
}
