import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Alert } from '@/components/feedback/Alert'
import { Spinner } from '@/components/feedback/Spinner'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { StatsCard } from '@/components/ui/StatsCard'
import { AccountStatus, UserRole } from '@/features/auth/types/authTypes'
import { getProsumers } from '@/features/prosumers/api/prosumersApi'
import type { Prosumer } from '@/features/prosumers/types/prosumerTypes'
import { searchReservations } from '@/features/reservations/api/reservationsApi'
import { ReservationStatus, type Reservation } from '@/features/reservations/types/reservationTypes'
import { getStations } from '@/features/stations/api/stationsApi'
import { StationStatus, type Station } from '@/features/stations/types/stationTypes'
import { getUsers } from '@/features/users/api/usersApi'
import type { WebUser } from '@/features/users/types/userTypes'
import { PageContent, PageHeader } from '@/layouts/PageLayout'
import { formatDateTime } from '@/utils/formatDate'

interface DashboardData { users: WebUser[]; prosumers: Prosumer[]; stations: Station[]; reservations: Reservation[] }
const emptyData: DashboardData = { users: [], prosumers: [], stations: [], reservations: [] }

export function BackofficeDashboardPage() {
  const [data, setData] = useState<DashboardData>(emptyData)
  const [isLoading, setIsLoading] = useState(true)
  const [hasPartialData, setHasPartialData] = useState(false)
  const [requestVersion, setRequestVersion] = useState(0)

  const refresh = useCallback(() => { setIsLoading(true); setRequestVersion((version) => version + 1) }, [])

  useEffect(() => {
    const controller = new AbortController()
    Promise.allSettled([getUsers(controller.signal), getProsumers(controller.signal), getStations(controller.signal), searchReservations({}, controller.signal)])
      .then(([users, prosumers, stations, reservations]) => {
        if (controller.signal.aborted) return
        setData({ users: users.status === 'fulfilled' ? users.value : [], prosumers: prosumers.status === 'fulfilled' ? prosumers.value : [], stations: stations.status === 'fulfilled' ? stations.value : [], reservations: reservations.status === 'fulfilled' ? reservations.value : [] })
        setHasPartialData([users, prosumers, stations, reservations].some((result) => result.status === 'rejected'))
      }).finally(() => { if (!controller.signal.aborted) setIsLoading(false) })
    return () => controller.abort()
  }, [requestVersion])

  const activity = useMemo(() => [
    ...data.prosumers.map((item) => ({ id: `p-${item.userId}`, title: `${item.firstName} ${item.lastName}`, detail: `Prosumer account · ${item.accountStatus}`, date: item.updatedAt, href: `/prosumers/${item.userId}` })),
    ...data.reservations.map((item) => ({ id: `r-${item.reservationId}`, title: `Reservation ${item.reservationId.slice(-8)}`, detail: `Reservation · ${item.status}`, date: item.updatedAt, href: `/reservations/${item.reservationId}` })),
    ...data.stations.map((item) => ({ id: `s-${item.id}`, title: item.name, detail: `Microgrid node · ${item.status}`, date: item.updatedAt, href: `/stations/${item.id}` })),
  ].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()).slice(0, 6), [data])

  const activeUsers = data.users.filter((user) => user.accountStatus === AccountStatus.ACTIVE).length
  const pendingProsumers = data.prosumers.filter((prosumer) => prosumer.accountStatus === AccountStatus.PENDING).length
  const operators = data.users.filter((user) => user.role === UserRole.GRID_OPERATOR && user.accountStatus === AccountStatus.ACTIVE).length
  const activeReservations = data.reservations.filter((reservation) => reservation.status === ReservationStatus.PENDING || reservation.status === ReservationStatus.APPROVED).length
  const activeStations = data.stations.filter((station) => station.status === StationStatus.ACTIVE).length

  return <>
    <PageHeader title="Backoffice Dashboard" description="A live overview of accounts, microgrid infrastructure, and reservation approvals." actions={<Button variant="outline" onClick={refresh} disabled={isLoading}>Refresh data</Button>} />
    <PageContent>
      {hasPartialData && <Alert variant="warning" title="Some dashboard data is unavailable">Available sections are shown below. Refresh to retry the missing services.</Alert>}
      {isLoading ? <div className="page-loading"><Spinner label="Loading dashboard" /></div> : <>
        <div className="stats-grid" aria-label="System overview">
          <StatsCard label="Web users" value={data.users.length} detail={`${activeUsers} active accounts`} action={<Link to="/users">Manage users</Link>} />
          <StatsCard label="Pending Prosumers" value={pendingProsumers} detail={`${data.prosumers.length} total Prosumer accounts`} tone="warning" action={<Link to="/prosumers/pending">Review approvals</Link>} />
          <StatsCard label="Active operators" value={operators} detail="Grid Operator accounts with access" tone="info" action={<Link to="/users">View operators</Link>} />
          <StatsCard label="Active reservations" value={activeReservations} detail={`${data.reservations.length} reservations in total`} tone="success" action={<Link to="/reservations">Manage reservations</Link>} />
          <StatsCard label="Microgrid nodes" value={data.stations.length} detail={`${activeStations} currently active`} action={<Link to="/stations">Manage nodes</Link>} />
          <StatsCard label="Pending approvals" value={data.reservations.filter((item) => item.status === ReservationStatus.PENDING).length} detail="Reservations awaiting Backoffice" tone="warning" action={<Link to="/reservations">Review queue</Link>} />
          <StatsCard label="Completed" value={data.reservations.filter((item) => item.status === ReservationStatus.COMPLETED).length} detail="Completed energy transfers" tone="success" />
          <StatsCard label="Approved" value={data.reservations.filter((item) => item.status === ReservationStatus.APPROVED).length} detail="Ready for operator verification" tone="info" />
        </div>
        <div className="dashboard-grid">
          <Card title="Recent activity" description="Latest updates across reservations, Prosumers, and microgrid nodes.">
            {activity.length === 0 ? <p className="muted-copy">No recent system activity is available.</p> : <ul className="activity-list">{activity.map((item) => <li key={item.id}><div><Link to={item.href}><strong>{item.title}</strong></Link><span>{item.detail}</span></div><time dateTime={item.date}>{formatDateTime(item.date)}</time></li>)}</ul>}
          </Card>
          <Card title="Quick actions" description="Frequently used Backoffice workflows."><div className="quick-actions">
            <Link className="button button--primary" to="/prosumers/pending">Review Prosumer approvals {pendingProsumers > 0 && <Badge variant="warning">{pendingProsumers}</Badge>}</Link>
            <Link className="button button--outline" to="/users/new">Create Web user</Link><Link className="button button--outline" to="/stations/new">Create microgrid node</Link><Link className="button button--outline" to="/reservations">Review reservations</Link>
          </div></Card>
        </div>
      </>}
    </PageContent>
  </>
}
