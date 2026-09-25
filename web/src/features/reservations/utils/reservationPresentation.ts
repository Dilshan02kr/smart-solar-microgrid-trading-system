import type { BadgeVariant } from '@/components/ui/Badge'
import { ReservationStatus, type ReservationStatus as ReservationStatusValue } from '@/features/reservations/types/reservationTypes'

export function getReservationStatusBadgeVariant(status: ReservationStatusValue): BadgeVariant {
  switch (status) {
    case ReservationStatus.PENDING:
      return 'warning'
    case ReservationStatus.APPROVED:
      return 'info'
    case ReservationStatus.COMPLETED:
      return 'success'
    case ReservationStatus.CANCELLED:
      return 'danger'
  }
}

export function getReservationStatusLabel(status: ReservationStatusValue): string {
  return status.charAt(0) + status.slice(1).toLowerCase()
}
