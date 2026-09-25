import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { updateSlot } from '@/features/slots/api/slotsApi'
import { SlotForm } from '@/features/slots/components/SlotForm'
import { useSlot } from '@/features/slots/hooks/useSlot'
import type { UpdateEnergyBookingSlotRequest } from '@/features/slots/types/slotTypes'
import { useStation } from '@/features/stations/hooks/useStation'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function EditSlotPage() {
  const { stationId = '', slotId = '' } = useParams()
  const navigate = useNavigate()
  const stationState = useStation(stationId)
  const slotState = useSlot(slotId)
  const [apiError, setApiError] = useState<ApiClientError | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [missingSlotId, setMissingSlotId] = useState<string | null>(null)

  function retry() {
    stationState.retry()
    slotState.retry()
  }

  async function handleSubmit(request: UpdateEnergyBookingSlotRequest) {
    if (!stationState.station || !slotState.slot || isSubmitting) return
    setApiError(null)
    setIsSubmitting(true)
    try {
      await updateSlot(slotState.slot.id, request)
      navigate(`/stations/${stationState.station.id}/slots`, { replace: true, state: { success: 'The energy booking slot was updated successfully.' } })
    } catch (requestError) {
      const normalized = toApiClientError(requestError)
      if (normalized.status === 404) setMissingSlotId(slotState.slot.id)
      else setApiError(normalized)
    } finally {
      setIsSubmitting(false)
    }
  }

  const isLoading = stationState.isLoading || slotState.isLoading
  const notFound = stationState.notFound || slotState.notFound || missingSlotId === slotId
  const error = stationState.error ?? slotState.error
  const routeMismatch = Boolean(slotState.slot && slotState.slot.stationId !== stationId)

  return (
    <>
      <PageHeader title={stationState.station ? `Edit Energy Slot — ${stationState.station.name}` : 'Edit Energy Slot'} description="Update the slot date, time range, and capacity. Availability remains server-controlled." />
      <PageContent>
        {isLoading ? <div className="page-loading"><Spinner label="Loading energy slot" /></div>
          : notFound || routeMismatch ? <EmptyState title="Energy slot not found" description={routeMismatch ? 'This energy slot does not belong to the microgrid node in the current route.' : 'The requested energy slot or microgrid node could not be found.'} action={<Link className="button button--primary" to={`/stations/${stationId}/slots`}>Back to Energy Slots</Link>} />
            : error ? <PageError message={error.message} onRetry={retry} />
              : stationState.station && slotState.slot ? <SlotForm key={slotState.slot.id} slot={slotState.slot} stationCapacityKw={stationState.station.totalCapacityKw} apiError={apiError} isSubmitting={isSubmitting} cancelTo={`/stations/${stationId}/slots`} submitLabel="Save changes" onSubmit={(request) => void handleSubmit(request)} /> : null}
      </PageContent>
    </>
  )
}
