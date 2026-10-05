import { Link } from 'react-router-dom'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { DataTable } from '@/components/ui/DataTable'
import { StationStatus, type Station } from '@/features/stations/types/stationTypes'
import { formatCapacity, getStationStatusBadgeVariant } from '@/features/stations/utils/stationPresentation'

export interface StationsTableProps {
  stations: Station[]
  onStatusChange: (station: Station) => void
  operatorNames?: ReadonlyMap<string, string>
}

export function StationsTable({ stations, onStatusChange, operatorNames = new Map() }: StationsTableProps) {
  return (
    <DataTable caption="Microgrid nodes">
      <thead><tr><th scope="col">Name</th><th scope="col">Location</th><th scope="col">Grid Operator</th><th scope="col">Capacity</th><th scope="col">Operational schedule</th><th scope="col">Status</th><th scope="col">Actions</th></tr></thead>
      <tbody>
        {stations.map((station) => (
          <tr key={station.id}>
            <td><strong>{station.name}</strong></td>
            <td>{station.locationName}</td>
            <td>{operatorNames.get(station.id) ?? <Badge variant="warning">Not assigned</Badge>}</td>
            <td>{formatCapacity(station.totalCapacityKw)}</td>
            <td>{station.operationalSchedule}</td>
            <td><Badge variant={getStationStatusBadgeVariant(station.status)}>{station.status}</Badge></td>
            <td><div className="table-actions">
              <Link className="button button--ghost" to={`/stations/${station.id}`}>View</Link>
              <Link className="button button--ghost" to={`/stations/${station.id}/edit`}>Edit</Link>
              <Button variant="ghost" onClick={() => onStatusChange(station)}>{station.status === StationStatus.ACTIVE ? 'Deactivate' : 'Activate'}</Button>
            </div></td>
          </tr>
        ))}
      </tbody>
    </DataTable>
  )
}
