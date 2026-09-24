import axios from 'axios'
import type { ApiErrorResponse } from '@/services/api/apiTypes'

export class ApiClientError extends Error {
  readonly status?: number
  readonly code: string
  readonly fieldErrors?: Record<string, string[]> | null

  constructor(message: string, code = 'UNEXPECTED_ERROR', status?: number, fieldErrors?: Record<string, string[]> | null) {
    super(message)
    this.name = 'ApiClientError'
    this.status = status
    this.code = code
    this.fieldErrors = fieldErrors
  }
}

function isApiErrorResponse(value: unknown): value is ApiErrorResponse {
  if (typeof value !== 'object' || value === null) return false
  const candidate = value as Partial<ApiErrorResponse>
  return typeof candidate.code === 'string' && typeof candidate.message === 'string'
}

export function toApiClientError(error: unknown): ApiClientError {
  if (error instanceof ApiClientError) return error

  if (!axios.isAxiosError(error)) {
    return new ApiClientError('An unexpected error occurred. Please try again.')
  }

  if (error.code === 'ECONNABORTED' || error.code === 'ETIMEDOUT') {
    return new ApiClientError('The request timed out. Please try again.', 'REQUEST_TIMEOUT')
  }

  if (!error.response) {
    return new ApiClientError('The service is currently unreachable. Check your connection and try again.', 'NETWORK_UNAVAILABLE')
  }

  const { status, data } = error.response
  if (isApiErrorResponse(data)) {
    return new ApiClientError(data.message, data.code, status, data.errors)
  }

  if (status === 401) {
    return new ApiClientError('Authentication is required.', 'AUTHENTICATION_REQUIRED', status)
  }

  if (status === 403) {
    return new ApiClientError('You do not have permission to perform this action.', 'ACCESS_DENIED', status)
  }

  return new ApiClientError('The server returned an unexpected response. Please try again.', 'UNEXPECTED_RESPONSE', status)
}
