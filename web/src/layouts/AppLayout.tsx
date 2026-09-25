import { useEffect, useState } from 'react'
import { Outlet, useLocation } from 'react-router-dom'
import { Sidebar } from '@/components/navigation/Sidebar'
import { Topbar } from '@/components/navigation/Topbar'
import { useAuth } from '@/features/auth/hooks/useAuth'
import { getRoleLabel } from '@/features/auth/utils/rolePresentation'

const routeTitles: Record<string, string> = {
  '/': 'Dashboard',
  '/components': 'Component showcase',
  '/users': 'Web users',
  '/users/new': 'Create Web user',
  '/prosumers': 'Prosumers',
  '/prosumers/pending': 'Pending registrations',
  '/stations': 'Microgrid Nodes',
  '/stations/new': 'Create microgrid node',
}

function getRouteTitle(pathname: string): string {
  if (routeTitles[pathname]) return routeTitles[pathname]
  if (/^\/users\/[^/]+\/edit$/.test(pathname)) return 'Edit Web user'
  if (/^\/prosumers\/[^/]+$/.test(pathname)) return 'Prosumer details'
  if (/^\/stations\/[^/]+\/slots\/new$/.test(pathname)) return 'Add Energy Slot'
  if (/^\/stations\/[^/]+\/slots\/[^/]+\/edit$/.test(pathname)) return 'Edit Energy Slot'
  if (/^\/stations\/[^/]+\/slots$/.test(pathname)) return 'Energy Slots'
  if (/^\/stations\/[^/]+\/edit$/.test(pathname)) return 'Edit microgrid node'
  if (/^\/stations\/[^/]+$/.test(pathname)) return 'Microgrid node details'
  return 'Smart Solar Microgrid'
}

export function AppLayout() {
  const [navigationOpen, setNavigationOpen] = useState(false)
  const location = useLocation()
  const { user, logout } = useAuth()

  useEffect(() => {
    if (!navigationOpen) return
    document.getElementById('app-sidebar')?.focus()
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setNavigationOpen(false)
        document.getElementById('navigation-menu-button')?.focus()
      }
    }
    document.addEventListener('keydown', closeOnEscape)
    return () => document.removeEventListener('keydown', closeOnEscape)
  }, [navigationOpen])

  if (!user) return null

  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">Skip to main content</a>
      <Sidebar open={navigationOpen} onNavigate={() => setNavigationOpen(false)} role={user.role} />
      {navigationOpen && <button className="sidebar-backdrop" type="button" aria-label="Close navigation menu" onClick={() => {
        setNavigationOpen(false)
        document.getElementById('navigation-menu-button')?.focus()
      }} />}
      <div className="app-shell__main">
        <Topbar
          title={getRouteTitle(location.pathname)}
          menuOpen={navigationOpen}
          onMenuClick={() => setNavigationOpen(true)}
          userName={`${user.firstName} ${user.lastName}`.trim()}
          roleLabel={getRoleLabel(user.role)}
          onLogout={logout}
        />
        <main id="main-content" className="app-content" tabIndex={-1}>
          <Outlet />
        </main>
      </div>
    </div>
  )
}
