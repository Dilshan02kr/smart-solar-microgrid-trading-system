import { Alert } from '@/components/feedback/Alert'
import { EmptyState } from '@/components/feedback/EmptyState'
import { Spinner } from '@/components/feedback/Spinner'
import { Badge, type BadgeVariant } from '@/components/ui/Badge'
import { Button, type ButtonVariant } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { PageContent, PageHeader } from '@/layouts/PageLayout'

const buttonVariants: ButtonVariant[] = ['primary', 'secondary', 'outline', 'danger', 'ghost']
const badgeVariants: BadgeVariant[] = ['default', 'success', 'warning', 'danger', 'info']

export function ComponentShowcasePage() {
  return (
    <>
      <PageHeader title="Component showcase" description="A development-only reference for the shared, domain-neutral UI foundation." actions={<Button variant="outline">Example action</Button>} />
      <PageContent>
        <section className="showcase-section" aria-labelledby="buttons-title">
          <h2 id="buttons-title" className="showcase-section__title">Buttons and badges</h2>
          <Card>
            <div className="stack">
              <div className="cluster">{buttonVariants.map((variant) => <Button key={variant} variant={variant}>{variant}</Button>)}</div>
              <div className="cluster"><Button loading>Saving</Button><Button disabled>Disabled</Button></div>
              <div className="cluster">{badgeVariants.map((variant) => <Badge key={variant} variant={variant}>{variant}</Badge>)}</div>
            </div>
          </Card>
        </section>

        <section className="showcase-section" aria-labelledby="fields-title">
          <h2 id="fields-title" className="showcase-section__title">Form controls</h2>
          <Card>
            <div className="showcase-controls">
              <Input label="Display name" placeholder="Enter a name" helperText="A short, descriptive label." required />
              <Input label="Reference" defaultValue="Invalid example" error="Review this field before continuing." />
              <Select label="Interface preference" placeholder="Choose an option" options={[{ label: 'Compact', value: 'compact' }, { label: 'Comfortable', value: 'comfortable' }]} />
              <Select label="Unavailable control" options={[{ label: 'Example', value: 'example' }]} disabled />
            </div>
          </Card>
        </section>

        <section className="showcase-section" aria-labelledby="feedback-title">
          <h2 id="feedback-title" className="showcase-section__title">Feedback</h2>
          <div className="showcase-grid">
            <div className="stack">
              <Alert variant="success" title="Success">The requested action completed.</Alert>
              <Alert variant="error" title="Action needed">A problem requires attention.</Alert>
              <Alert variant="warning" title="Check details">Review the information before continuing.</Alert>
              <Alert variant="info" title="Information">This message provides helpful context.</Alert>
            </div>
            <Card title="Loading and empty states">
              <div className="stack">
                <Spinner label="Loading content" />
                <EmptyState title="Nothing here yet" description="Content will appear here when it becomes available." action={<Button variant="outline">Example action</Button>} />
              </div>
            </Card>
          </div>
        </section>
      </PageContent>
    </>
  )
}
