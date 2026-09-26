export const StationStatus = {
  ACTIVE: 'ACTIVE',
  INACTIVE: 'INACTIVE',
} as const

export type StationStatus = (typeof StationStatus)[keyof typeof StationStatus]

export interface Station {
  id: string
  name: string
  locationName: string
  latitude: number
  longitude: number
  totalCapacityKw: number
  operationalSchedule: string
  status: StationStatus
  createdAt: string
  updatedAt: string
}

export interface CreateStationRequest {
  name: string
  locationName: string
  latitude: number
  longitude: number
  totalCapacityKw: number
  operationalSchedule: string
}

export type UpdateStationRequest = CreateStationRequest
