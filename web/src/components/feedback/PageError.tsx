import { Alert } from '@/components/feedback/Alert'
import { Button } from '@/components/ui/Button'

export interface PageErrorProps {
  message: string
  onRetry?: () => void
}

export function PageError({ message, onRetry }: PageErrorProps) {
  return (
    <div className="page-error">
      <Alert variant="error" title="Unable to load this page">{message}</Alert>
      {onRetry && <Button variant="outline" onClick={onRetry}>Try again</Button>}
    </div>
  )
}
