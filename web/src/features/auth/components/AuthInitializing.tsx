import { Spinner } from '@/components/feedback/Spinner'

export function AuthInitializing() {
  return (
    <main className="auth-initializing" aria-label="Restoring session">
      <Spinner label="Restoring your session" />
    </main>
  )
}
