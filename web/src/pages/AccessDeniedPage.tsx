import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { useAuth } from '@/features/auth/hooks/useAuth'
import { isWebAppRole } from '@/features/auth/types/authTypes'

export function AccessDeniedPage() {
  const { user, logout } = useAuth()

  return (
    <main className="centered-page">
      <Card className="centered-page__panel">
        <p className="centered-page__eyebrow">Access denied</p>
        <h1 className="centered-page__title">You do not have permission to view this page</h1>
        <p className="centered-page__description">Your account remains signed in. Return to an available page or sign out to use another account.</p>
        <div className="cluster">
          {user && isWebAppRole(user.role) && <Link className="button button--primary" to="/">Return home</Link>}
          <Button variant="outline" onClick={logout}>Log out</Button>
        </div>
      </Card>
    </main>
  )
}
