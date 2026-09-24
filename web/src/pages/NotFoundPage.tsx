import { Link } from 'react-router-dom'
import { Card } from '@/components/ui/Card'

export function NotFoundPage() {
  return (
    <section className="centered-page" aria-labelledby="not-found-title">
      <Card className="centered-page__panel">
        <p className="centered-page__eyebrow">404</p>
        <h1 id="not-found-title" className="centered-page__title">Page not found</h1>
        <p className="centered-page__description">The address does not match an available foundation route.</p>
        <Link className="button button--primary" to="/">Return home</Link>
      </Card>
    </section>
  )
}
