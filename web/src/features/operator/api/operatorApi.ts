import type { AxiosResponse } from 'axios'
import type {
  CompleteReservationResponse,
  DashboardSummaryResponse,
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

export function verifyTransaction(request: VerifyTransactionRequest): Promise<VerifyTransactionResponse> {
  return dataOrNormalizedError(apiClient.post<VerifyTransactionResponse>('/api/operator/verify-transaction', request))
}

export function completeReservation(reservationId: string): Promise<CompleteReservationResponse> {
  return dataOrNormalizedError(apiClient.post<CompleteReservationResponse>(`/api/operator/reservations/${encodeURIComponent(reservationId)}/complete`))
}
