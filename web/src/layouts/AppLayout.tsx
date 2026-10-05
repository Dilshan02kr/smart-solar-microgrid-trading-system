import { useEffect, useState } from 'react'
import { Outlet, useLocation } from 'react-router-dom'
import { Sidebar } from '@/components/navigation/Sidebar'
import { Topbar } from '@/components/navigation/Topbar'
import { useAuth } from '@/features/auth/hooks/useAuth'
import { getRoleLabel } from '@/features/auth/utils/rolePresentation'

const routeTitles: Record<string, string> = {
  '/': 'Dashboard',
  '/users': 'Web users',
  '/users/new': 'Create Web user',
  '/prosumers': 'Prosumers',
  '/prosumers/pending': 'Pending registrations',
  '/stations': 'Microgrid Nodes',
  '/stations/new': 'Create microgrid node',
  '/reservations': 'Reservations',
  '/operator/operations': 'Operator Operations',
  '/operator/reservations': 'Operational Reservations',
  '/404': 'Page not found',
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
  if (/^\/reservations\/[^/]+$/.test(pathname)) return 'Reservation details'
  return 'Smart Solar Microgrid'
}

export function AppLayout() {
  const [navigationOpen, setNavigationOpen] = useState(false)
  const [isNarrowViewport, setIsNarrowViewport] = useState(() => window.matchMedia('(max-width: 56rem)').matches)
  const location = useLocation()
  const { user, logout } = useAuth()

  useEffect(() => {
    const mediaQuery = window.matchMedia('(max-width: 56rem)')
    const updateViewport = (event: MediaQueryListEvent) => setIsNarrowViewport(event.matches)
    mediaQuery.addEventListener('change', updateViewport)
    return () => mediaQuery.removeEventListener('change', updateViewport)
  }, [])

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

  function handleNavigation() {
    setNavigationOpen(false)
    if (isNarrowViewport) window.requestAnimationFrame(() => document.getElementById('main-content')?.focus())
  }

  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">Skip to main content</a>
      <Sidebar open={navigationOpen} inertWhenClosed={isNarrowViewport && !navigationOpen} onNavigate={handleNavigation} role={user.role} />
      {navigationOpen && <button className="sidebar-backdrop" type="button" aria-label="Close navigation menu" onClick={() => {
        setNavigationOpen(false)
        document.getElementById('navigation-menu-button')?.focus()
      }} />}
      <div className="app-shell__main">
        <Topbar
          title={getRouteTitle(location.pathname)}
          menuOpen={navigationOpen}
          onMenuClick={() => setNavigationOpen((open) => !open)}
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
