import type { AxiosResponse } from 'axios'
import type {
  CompleteReservationResponse,
  DashboardSummaryResponse,
  OperatorReservation,
  OperatorSlot,
  UpdateOperatorSlotAvailabilityRequest,
  VerifyTransactionRequest,
  VerifyTransactionResponse,
} from '@/features/operator/types/operatorTypes'
import { apiClient } from '@/services/api/apiClient'
import { toApiClientError } from '@/services/api/apiError'

async function dataOrNormalizedError<T>(request: Promise<AxiosResponse<T>>): Promise<T> {
  try {
    return (await request).data
  } catch (error) {
    throw toApiClientError(error)
  }
}

export function getOperatorDashboardSummary(signal?: AbortSignal): Promise<DashboardSummaryResponse> {
  return dataOrNormalizedError(apiClient.get<DashboardSummaryResponse>('/api/operator/dashboard/summary', { signal }))
}

export function getOperatorReservations(signal?: AbortSignal): Promise<OperatorReservation[]> {
  return dataOrNormalizedError(apiClient.get<OperatorReservation[]>('/api/operator/reservations', { signal }))
}

export function getOperatorSlots(signal?: AbortSignal): Promise<OperatorSlot[]> {
  return dataOrNormalizedError(apiClient.get<OperatorSlot[]>('/api/operator/slots', { signal }))
}

export function updateOperatorSlotAvailability(
  slotId: string,
  request: UpdateOperatorSlotAvailabilityRequest,
): Promise<OperatorSlot> {
  return dataOrNormalizedError(apiClient.patch<OperatorSlot>(
    `/api/operator/slots/${encodeURIComponent(slotId)}/availability`,
    request,
  ))
}

export function verifyTransaction(request: VerifyTransactionRequest): Promise<VerifyTransactionResponse> {
  return dataOrNormalizedError(apiClient.post<VerifyTransactionResponse>('/api/operator/verify-transaction', request))
}

export function completeReservation(reservationId: string): Promise<CompleteReservationResponse> {
  return dataOrNormalizedError(apiClient.post<CompleteReservationResponse>(`/api/operator/reservations/${encodeURIComponent(reservationId)}/complete`))
}
