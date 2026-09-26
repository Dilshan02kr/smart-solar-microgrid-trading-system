import type { AxiosResponse } from 'axios'
import type { CreateEnergyBookingSlotRequest, EnergyBookingSlot, UpdateEnergyBookingSlotRequest } from '@/features/slots/types/slotTypes'
import { apiClient } from '@/services/api/apiClient'
import { toApiClientError } from '@/services/api/apiError'

async function dataOrNormalizedError<T>(request: Promise<AxiosResponse<T>>): Promise<T> {
  try {
    return (await request).data
  } catch (error) {
    throw toApiClientError(error)
  }
}

export function getSlots(signal?: AbortSignal): Promise<EnergyBookingSlot[]> {
  return dataOrNormalizedError(apiClient.get<EnergyBookingSlot[]>('/api/energy-booking-slots', { signal }))
}

export function getSlot(slotId: string, signal?: AbortSignal): Promise<EnergyBookingSlot> {
  return dataOrNormalizedError(apiClient.get<EnergyBookingSlot>(`/api/energy-booking-slots/${encodeURIComponent(slotId)}`, { signal }))
}

export function getSlotsByStation(stationId: string, signal?: AbortSignal): Promise<EnergyBookingSlot[]> {
  return dataOrNormalizedError(apiClient.get<EnergyBookingSlot[]>(`/api/energy-booking-slots/station/${encodeURIComponent(stationId)}`, { signal }))
}

export function createSlot(request: CreateEnergyBookingSlotRequest): Promise<EnergyBookingSlot> {
  return dataOrNormalizedError(apiClient.post<EnergyBookingSlot>('/api/energy-booking-slots', request))
}

export function updateSlot(slotId: string, request: UpdateEnergyBookingSlotRequest): Promise<EnergyBookingSlot> {
  return dataOrNormalizedError(apiClient.put<EnergyBookingSlot>(`/api/energy-booking-slots/${encodeURIComponent(slotId)}`, request))
}
