import type { ApiClientError } from '@/services/api/apiError'

export const OPERATOR_STATION_NOT_ASSIGNED_MESSAGE = 'Your Grid Operator account is not currently assigned to a microgrid node. Contact a Backoffice administrator.'
export const OPERATOR_ACCESS_DENIED_MESSAGE = 'This transaction is not available for your assigned microgrid node.'

export function getOperatorErrorMessage(error: ApiClientError): string {
  if (error.code === 'OPERATOR_STATION_NOT_ASSIGNED') return OPERATOR_STATION_NOT_ASSIGNED_MESSAGE
  if (error.code === 'ACCESS_DENIED') return OPERATOR_ACCESS_DENIED_MESSAGE
  return error.message
}
