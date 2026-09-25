import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/Button'
import { Badge } from '@/components/ui/Badge'
import { DataTable } from '@/components/ui/DataTable'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { getAccountStatusBadgeVariant } from '@/utils/accountStatusPresentation'

export interface ProsumersTableProps {
  prosumers: Prosumer[]
  onActivate?: (prosumer: Prosumer) => void
}

export function ProsumersTable({ prosumers, onActivate }: ProsumersTableProps) {
  return (
    <DataTable caption="Prosumer accounts">
      <thead>
        <tr><th scope="col">Name</th><th scope="col">NIC</th><th scope="col">Email</th><th scope="col">Phone</th><th scope="col">Status</th><th scope="col">Actions</th></tr>
      </thead>
      <tbody>
        {prosumers.map((prosumer) => (
          <tr key={prosumer.userId}>
            <td><strong>{prosumer.firstName} {prosumer.lastName}</strong></td>
            <td>{prosumer.nic}</td>
            <td>{prosumer.email}</td>
            <td>{prosumer.phone}</td>
            <td><Badge variant={getAccountStatusBadgeVariant(prosumer.accountStatus)}>{prosumer.accountStatus}</Badge></td>
            <td>
              <div className="table-actions">
                <Link className="button button--ghost" to={`/prosumers/${prosumer.userId}`}>View details</Link>
                {onActivate && <Button variant="ghost" onClick={() => onActivate(prosumer)}>Activate</Button>}
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </DataTable>
  )
}
