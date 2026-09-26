import type { ApiClientError } from '@/services/api/apiError'

export function getApiFieldError(error: ApiClientError | null, fieldName: string): string | undefined {
  if (!error?.fieldErrors) return undefined
  const normalizedName = fieldName.toLowerCase()
  const match = Object.entries(error.fieldErrors).find(([key]) => {
    const finalSegment = key.split('.').at(-1)?.toLowerCase()
    return finalSegment === normalizedName
  })
  return match?.[1]?.[0]
}
