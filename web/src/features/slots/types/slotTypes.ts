export interface EnergyBookingSlot {
  id: string
  stationId: string
  date: string
  startTime: string
  endTime: string
  capacityKw: number
  isAvailable: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateEnergyBookingSlotRequest {
  stationId: string
  date: string
  startTime: string
  endTime: string
  capacityKw: number
}

export interface UpdateEnergyBookingSlotRequest {
  date: string
  startTime: string
  endTime: string
  capacityKw: number
}
