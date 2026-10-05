import { useEffect, useState } from 'react'
import { Alert } from '@/components/feedback/Alert'
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { getOperatorSlots, updateOperatorSlotAvailability } from '@/features/operator/api/operatorApi'
import type { OperatorSlot } from '@/features/operator/types/operatorTypes'
import { formatSlotDate, formatSlotTime } from '@/features/slots/utils/slotPresentation'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function OperatorSlotsPage() {
  const [slots, setSlots] = useState<OperatorSlot[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<ApiClientError | null>(null)
  const [selected, setSelected] = useState<OperatorSlot | null>(null)
  const [mutationError, setMutationError] = useState<string>()
  const [isMutating, setIsMutating] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)

  function refresh() {
    setIsLoading(true)
    setLoadError(null)
    setRequestVersion((value) => value + 1)
  }

  useEffect(() => {
    const controller = new AbortController()
    getOperatorSlots(controller.signal)
      .then((response) => {
        setSlots(response)
        setLoadError(null)
      })
      .catch((error: unknown) => {
        if (!controller.signal.aborted) setLoadError(toApiClientError(error))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })
    return () => controller.abort()
  }, [requestVersion])

  async function confirmUpdate() {
    if (!selected || isMutating) return
    setIsMutating(true)
    setMutationError(undefined)
    try {
      const updated = await updateOperatorSlotAvailability(selected.id, { isAvailable: !selected.isAvailable })
      setSlots((current) => current.map((slot) => slot.id === updated.id ? updated : slot))
      setSelected(null)
      refresh()
    } catch (error) {
      const normalized = toApiClientError(error)
      setMutationError(normalized.code === 'SLOT_HAS_ACTIVE_RESERVATION'
        ? 'This slot cannot be made available while an active reservation exists.'
        : normalized.message)
    } finally {
      setIsMutating(false)
    }
  }

  const unassigned = loadError?.code === 'OPERATOR_STATION_NOT_ASSIGNED'

  return (
    <>
      <PageHeader
        title="Energy Slot Availability"
        description="Manage availability for existing slots at your assigned microgrid node. Slot schedule and capacity remain controlled by Backoffice."
        actions={<Button variant="outline" onClick={refresh} disabled={isLoading || isMutating}>Refresh</Button>}
      />
      <PageContent>
        {isLoading ? (
          <div className="page-loading"><Spinner label="Loading assigned energy slots" /></div>
        ) : unassigned ? (
          <Alert variant="warning" title="Microgrid node assignment required">Your Grid Operator account is not assigned to a microgrid node. Contact a Backoffice administrator.</Alert>
        ) : loadError ? (
          <PageError message={loadError.message} onRetry={refresh} />
        ) : slots.length === 0 ? (
          <EmptyState title="No energy slots" description="Backoffice has not configured any slots for your assigned microgrid node." />
        ) : (
          <div className="table-scroll">
            <table className="data-table">
              <thead><tr><th>Date</th><th>Start time</th><th>End time</th><th>Capacity</th><th>Availability</th><th>Action</th></tr></thead>
              <tbody>{slots.map((slot) => (
                <tr key={slot.id}>
                  <td>{formatSlotDate(slot.date)}</td>
                  <td>{formatSlotTime(slot.startTime)}</td>
                  <td>{formatSlotTime(slot.endTime)}</td>
                  <td>{slot.capacityKw.toLocaleString()} kW</td>
                  <td><Badge variant={slot.isAvailable ? 'success' : 'warning'}>{slot.isAvailable ? 'Available' : 'Unavailable'}</Badge></td>
                  <td><Button variant="outline" disabled={isMutating} onClick={() => { setMutationError(undefined); setSelected(slot) }}>{slot.isAvailable ? 'Mark Unavailable' : 'Mark Available'}</Button></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
        )}
      </PageContent>
      <ConfirmationDialog
        open={selected !== null}
        title={selected?.isAvailable ? 'Mark slot unavailable?' : 'Mark slot available?'}
        description={selected?.isAvailable
          ? 'Prosumers will no longer be able to book this slot.'
          : 'The server will allow this only when no pending or approved reservation owns the slot.'}
        confirmLabel={selected?.isAvailable ? 'Mark Unavailable' : 'Mark Available'}
        confirmVariant={selected?.isAvailable ? 'danger' : 'primary'}
        errorMessage={mutationError}
        loading={isMutating}
        onCancel={() => { if (!isMutating) { setSelected(null); setMutationError(undefined) } }}
        onConfirm={() => void confirmUpdate()}
      />
    </>
  )
}
