import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { EmptyState } from '@/components/feedback/EmptyState'
import { PageError } from '@/components/feedback/PageError'
import { Spinner } from '@/components/feedback/Spinner'
import { updateStation } from '@/features/stations/api/stationsApi'
import { StationForm } from '@/features/stations/components/StationForm'
import { useStation } from '@/features/stations/hooks/useStation'
import type { UpdateStationRequest } from '@/features/stations/types/stationTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function EditStationPage() {
  const { stationId = '' } = useParams()
  const navigate = useNavigate()
  const { station, isLoading, notFound, error, retry } = useStation(stationId)
  const [apiError, setApiError] = useState<ApiClientError | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [missingStationId, setMissingStationId] = useState<string | null>(null)

  async function handleSubmit(request: UpdateStationRequest) {
    if (!station || isSubmitting) return
    setApiError(null)
    setIsSubmitting(true)
    try {
      await updateStation(station.id, request)
      navigate(`/stations/${station.id}`, { replace: true, state: { success: 'The microgrid node was updated successfully.' } })
    } catch (requestError) {
      const normalized = toApiClientError(requestError)
      if (normalized.status === 404) setMissingStationId(station.id)
      else setApiError(normalized)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <PageHeader title="Edit microgrid node" description="Update mutable station details. Status is managed separately." />
      <PageContent>
        {isLoading ? <div className="page-loading"><Spinner label="Loading microgrid node" /></div>
          : notFound || missingStationId === stationId ? <EmptyState title="Microgrid node not found" description="The requested node could not be found." action={<Link className="button button--primary" to="/stations">Back to Microgrid Nodes</Link>} />
            : error ? <PageError message={error.message} onRetry={retry} />
              : station ? <StationForm key={station.id} station={station} apiError={apiError} isSubmitting={isSubmitting} cancelTo={`/stations/${station.id}`} submitLabel="Save changes" onSubmit={(request) => void handleSubmit(request)} /> : null}
      </PageContent>
    </>
  )
}
