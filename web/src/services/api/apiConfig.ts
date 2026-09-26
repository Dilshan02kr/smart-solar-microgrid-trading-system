const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim()

if (!configuredBaseUrl && import.meta.env.DEV) {
  throw new Error('VITE_API_BASE_URL is required. Copy .env.example to .env and configure the API origin.')
}

export const apiBaseUrl = (configuredBaseUrl ?? '').replace(/\/+$/, '')
