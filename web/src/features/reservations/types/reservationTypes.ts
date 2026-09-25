export const ReservationStatus = {
  PENDING: 'PENDING',
  APPROVED: 'APPROVED',
  COMPLETED: 'COMPLETED',
  CANCELLED: 'CANCELLED',
} as const

export type ReservationStatus = (typeof ReservationStatus)[keyof typeof ReservationStatus]

export interface Reservation {
  reservationId: string
  prosumerId: string
  stationId: string
  slotId: string
  scheduledTime: string
  status: ReservationStatus
  transactionReference: string | null
  createdAt: string
  updatedAt: string
  completedAt: string | null
}

export interface ReservationSearchParams {
  prosumerId?: string
  status?: ReservationStatus
  searchTerm?: string
}
