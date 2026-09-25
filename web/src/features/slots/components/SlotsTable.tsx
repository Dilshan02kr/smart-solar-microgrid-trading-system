import { Link } from 'react-router-dom'
import { Badge } from '@/components/ui/Badge'
import { DataTable } from '@/components/ui/DataTable'
import type { EnergyBookingSlot } from '@/features/slots/types/slotTypes'
import { formatSlotDate, formatSlotTime } from '@/features/slots/utils/slotPresentation'
import { formatCapacity } from '@/features/stations/utils/stationPresentation'

export interface SlotsTableProps {
  slots: EnergyBookingSlot[]
  stationId: string
}

export function SlotsTable({ slots, stationId }: SlotsTableProps) {
  return (
    <DataTable caption="Energy booking slots">
      <thead><tr><th scope="col">Date</th><th scope="col">Start time</th><th scope="col">End time</th><th scope="col">Capacity</th><th scope="col">Availability</th><th scope="col">Actions</th></tr></thead>
      <tbody>
        {slots.map((slot) => (
          <tr key={slot.id}>
            <td>{formatSlotDate(slot.date)}</td>
            <td>{formatSlotTime(slot.startTime)}</td>
            <td>{formatSlotTime(slot.endTime)}</td>
            <td>{formatCapacity(slot.capacityKw)}</td>
            <td><Badge variant={slot.isAvailable ? 'success' : 'warning'}>{slot.isAvailable ? 'Available' : 'Unavailable'}</Badge></td>
            <td><div className="table-actions"><Link className="button button--ghost" to={`/stations/${stationId}/slots/${slot.id}/edit`}>Edit</Link></div></td>
          </tr>
        ))}
      </tbody>
    </DataTable>
  )
}
