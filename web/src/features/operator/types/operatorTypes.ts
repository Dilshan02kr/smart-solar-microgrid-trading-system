import type { ReservationStatus } from '@/features/reservations/types/reservationTypes'

export interface DashboardSummaryResponse {
  pendingReservationCount: number
  approvedFutureReservationCount: number
}

export interface VerifyTransactionRequest {
  transactionReference: string
}

export interface VerifyTransactionResponse {
  reservationId: string
  transactionReference: string
  status: ReservationStatus
  prosumerId: string
  stationId: string
  slotId: string
  scheduledTime: string
}

export interface CompleteReservationResponse {
  reservationId: string
  transactionReference: string
  status: ReservationStatus
  prosumerId: string
  stationId: string
  slotId: string
  scheduledTime: string
}

export type OperatorTransaction = VerifyTransactionResponse | CompleteReservationResponse
