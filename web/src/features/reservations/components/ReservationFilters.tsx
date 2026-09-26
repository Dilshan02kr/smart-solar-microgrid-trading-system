import { useState, type FormEvent } from 'react'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import { Select, type SelectOption } from '@/components/ui/Select'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { ReservationStatus, type ReservationSearchParams } from '@/features/reservations/types/reservationTypes'

export interface ReservationFiltersProps {
  filters: ReservationSearchParams
  prosumers: Prosumer[]
  onApply: (filters: ReservationSearchParams) => void
}

const statusOptions: SelectOption[] = [
  { label: 'All statuses', value: '' },
  { label: 'Pending', value: ReservationStatus.PENDING },
  { label: 'Approved', value: ReservationStatus.APPROVED },
  { label: 'Completed', value: ReservationStatus.COMPLETED },
  { label: 'Cancelled', value: ReservationStatus.CANCELLED },
]

export function ReservationFilters({ filters, prosumers, onApply }: ReservationFiltersProps) {
  const [searchTerm, setSearchTerm] = useState(filters.searchTerm ?? '')
  const [status, setStatus] = useState(filters.status ?? '')
  const [prosumerId, setProsumerId] = useState(filters.prosumerId ?? '')

  const prosumerOptions: SelectOption[] = [
    { label: 'All Prosumers', value: '' },
    ...(prosumerId && !prosumers.some((prosumer) => prosumer.userId === prosumerId)
      ? [{ label: prosumerId, value: prosumerId }]
      : []),
    ...prosumers.map((prosumer) => ({
      label: `${prosumer.firstName} ${prosumer.lastName} - ${prosumer.nic}`,
      value: prosumer.userId,
    })),
  ]

  function apply(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const trimmedSearch = searchTerm.trim()
    onApply({
      ...(trimmedSearch ? { searchTerm: trimmedSearch } : {}),
      ...(status ? { status: status as ReservationStatus } : {}),
      ...(prosumerId ? { prosumerId } : {}),
    })
  }

  function clear() {
    setSearchTerm('')
    setStatus('')
    setProsumerId('')
    onApply({})
  }

  return (
    <Card title="Search reservations" description="Search by transaction reference or an exact reservation, Prosumer, station, or slot ID.">
      <form className="reservation-filters" onSubmit={apply}>
        <Input label="Reference or ID" value={searchTerm} onChange={(event) => setSearchTerm(event.target.value)} placeholder="Transaction reference or ObjectId" />
        <Select label="Status" options={statusOptions} value={status} onChange={(event) => setStatus(event.target.value)} />
        <Select label="Prosumer" options={prosumerOptions} value={prosumerId} onChange={(event) => setProsumerId(event.target.value)} />
        <div className="reservation-filters__actions">
          <Button type="button" variant="outline" onClick={clear}>Clear</Button>
          <Button type="submit">Apply</Button>
        </div>
      </form>
    </Card>
  )
}
