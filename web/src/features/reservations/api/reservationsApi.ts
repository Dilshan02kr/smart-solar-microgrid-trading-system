import type { AxiosResponse } from 'axios'
import type { Reservation, ReservationSearchParams } from '@/features/reservations/types/reservationTypes'
import { apiClient } from '@/services/api/apiClient'
import { toApiClientError } from '@/services/api/apiError'

async function dataOrNormalizedError<T>(request: Promise<AxiosResponse<T>>): Promise<T> {
  try {
    return (await request).data
  } catch (error) {
    throw toApiClientError(error)
  }
}

export function searchReservations(params: ReservationSearchParams, signal?: AbortSignal): Promise<Reservation[]> {
  return dataOrNormalizedError(apiClient.get<Reservation[]>('/api/reservations/search', { params, signal }))
}

export function getReservation(reservationId: string, signal?: AbortSignal): Promise<Reservation> {
  return dataOrNormalizedError(apiClient.get<Reservation>(`/api/reservations/${encodeURIComponent(reservationId)}`, { signal }))
}

export function getReservationsForProsumer(prosumerId: string, signal?: AbortSignal): Promise<Reservation[]> {
  return dataOrNormalizedError(apiClient.get<Reservation[]>(`/api/reservations/prosumer/${encodeURIComponent(prosumerId)}`, { signal }))
}

export function approveReservation(reservationId: string): Promise<Reservation> {
  return dataOrNormalizedError(apiClient.patch<Reservation>(`/api/reservations/${encodeURIComponent(reservationId)}/approve`))
}
