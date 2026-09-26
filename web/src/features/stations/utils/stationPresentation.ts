import type { BadgeVariant } from '@/components/ui/Badge'
import { StationStatus, type StationStatus as StationStatusValue } from '@/features/stations/types/stationTypes'

export function getStationStatusBadgeVariant(status: StationStatusValue): BadgeVariant {
  return status === StationStatus.ACTIVE ? 'success' : 'default'
}

export function formatCapacity(capacityKw: number): string {
  return `${new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(capacityKw)} kW`
}
