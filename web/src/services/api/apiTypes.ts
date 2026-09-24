import type { AxiosRequestConfig, InternalAxiosRequestConfig } from 'axios'

export interface ApiErrorResponse {
  code: string
  message: string
  errors?: Record<string, string[]> | null
}

interface UnauthorizedHandlingOptions {
  skipAuthentication?: boolean
  skipUnauthorizedHandling?: boolean
}

export type ApiRequestConfig = AxiosRequestConfig<unknown, unknown> & UnauthorizedHandlingOptions
export type InternalApiRequestConfig = InternalAxiosRequestConfig<unknown, unknown> & UnauthorizedHandlingOptions
