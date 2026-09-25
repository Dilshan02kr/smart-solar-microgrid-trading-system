import { Link } from 'react-router-dom'
import { Card } from '@/components/ui/Card'
import { PageContent, PageHeader } from '@/layouts/PageLayout'

export function DashboardPlaceholderPage() {
  return (
    <>
      <PageHeader title="Backoffice Dashboard" description="Manage accounts, microgrid infrastructure, and reservation approvals." />
      <PageContent>
        <Card className="dashboard-welcome" title="Backoffice workspace">
          <p>Choose a management area below. Dashboard metrics are not displayed because the Backoffice API does not provide an aggregate summary.</p>
        </Card>
        <div className="foundation-grid">
          <Card title="Web users"><p>Create and manage Backoffice and Grid Operator accounts.</p><Link to="/users">Manage users</Link></Card>
          <Card title="Prosumers"><p>Review Prosumer profiles and account activation states.</p><Link to="/prosumers">Manage Prosumers</Link></Card>
          <Card title="Microgrid Nodes"><p>Manage stations, operating details, and energy slots.</p><Link to="/stations">Manage nodes</Link></Card>
          <Card title="Reservations"><p>Monitor reservations and approve eligible pending requests.</p><Link to="/reservations">Manage reservations</Link></Card>
        </div>
      </PageContent>
    </>
  )
}
