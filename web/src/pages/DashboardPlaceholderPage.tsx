import { Link } from 'react-router-dom'
import { Card } from '@/components/ui/Card'
import { PageContent, PageHeader } from '@/layouts/PageLayout'

export function DashboardPlaceholderPage() {
  return (
    <>
      <PageHeader title="Web foundation" description="The shared application structure is ready for feature teams to build on in later stages." />
      <PageContent>
        <Card className="dashboard-welcome" title="Foundation stage">
          <p>This page intentionally contains no production data or business functionality. Visit the <Link to="/components">component showcase</Link> to review the shared interface primitives.</p>
        </Card>
        <div className="foundation-grid">
          <Card title="Consistent design"><p>Semantic tokens provide one visual language for color, spacing, typography, and layout.</p></Card>
          <Card title="Reusable structure"><p>The responsive shell, page layout, and accessible primitives can be composed by each feature team.</p></Card>
          <Card title="Ready for later stages"><p>Authentication, API integration, role filtering, and business screens remain intentionally unimplemented.</p></Card>
        </div>
      </PageContent>
    </>
  )
}
