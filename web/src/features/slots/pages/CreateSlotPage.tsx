import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { createSlot } from '@/features/slots/api/slotsApi'
import { SlotForm } from '@/features/slots/components/SlotForm'
import type { UpdateEnergyBookingSlotRequest } from '@/features/slots/types/slotTypes'
import { useStation } from '@/features/stations/hooks/useStation'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function CreateSlotPage() {
  const { stationId = '' } = useParams()
  const navigate = useNavigate()
  const { station, isLoading, notFound, error, retry } = useStation(stationId)
  const [apiError, setApiError] = useState<ApiClientError | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(request: UpdateEnergyBookingSlotRequest) {
    if (!station || isSubmitting) return
    setApiError(null)
    setIsSubmitting(true)
    try {
      await createSlot({ stationId: station.id, ...request })
      navigate(`/stations/${station.id}/slots`, { replace: true, state: { success: 'The energy booking slot was created successfully.' } })
    } catch (requestError) {
      setApiError(toApiClientError(requestError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <PageHeader title={station ? `Add Energy Slot — ${station.name}` : 'Add Energy Slot'} description="Create a dated capacity window. Availability is controlled by the server." />
      <PageContent>
        {isLoading ? <div className="page-loading"><Spinner label="Loading microgrid node" /></div>
          : notFound ? <EmptyState title="Microgrid node not found" description="The requested node could not be found." action={<Link className="button button--primary" to="/stations">Back to Microgrid Nodes</Link>} />
            : error ? <PageError message={error.message} onRetry={retry} />
              : station ? <SlotForm key={station.id} stationCapacityKw={station.totalCapacityKw} apiError={apiError} isSubmitting={isSubmitting} cancelTo={`/stations/${station.id}/slots`} submitLabel="Create slot" onSubmit={(request) => void handleSubmit(request)} /> : null}
      </PageContent>
    </>
  )
}
