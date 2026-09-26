import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createStation } from '@/features/stations/api/stationsApi'
import { StationForm } from '@/features/stations/components/StationForm'
import type { CreateStationRequest } from '@/features/stations/types/stationTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function CreateStationPage() {
  const navigate = useNavigate()
  const [apiError, setApiError] = useState<ApiClientError | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(request: CreateStationRequest) {
    if (isSubmitting) return
    setApiError(null)
    setIsSubmitting(true)
    try {
      const station = await createStation(request)
      navigate(`/stations/${station.id}`, { replace: true, state: { success: 'The microgrid node was created successfully.' } })
    } catch (requestError) {
      setApiError(toApiClientError(requestError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <PageHeader title="Create microgrid node" description="Add a station with its location, capacity, and operational information." />
      <PageContent><StationForm apiError={apiError} isSubmitting={isSubmitting} cancelTo="/stations" submitLabel="Create node" onSubmit={(request) => void handleSubmit(request)} /></PageContent>
    </>
  )
}
