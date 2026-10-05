import { NavLink } from 'react-router-dom'
import smartSolarLogo from '@/assets/branding/smart-solar-logo.png'
import { navigationItems } from '@/components/navigation/navigation'
import type { UserRole } from '@/features/auth/types/authTypes'

export interface SidebarProps {
  open: boolean
  inertWhenClosed: boolean
  onNavigate: () => void
  role: UserRole
}

export function Sidebar({ open, inertWhenClosed, onNavigate, role }: SidebarProps) {
  const visibleItems = navigationItems.filter((item) => item.allowedRoles.includes(role))

  return (
    <aside
      id="app-sidebar"
      className={`sidebar${open ? ' sidebar--open' : ''}`}
      aria-label="Primary navigation"
      aria-hidden={inertWhenClosed || undefined}
      inert={inertWhenClosed || undefined}
      tabIndex={-1}
    >
      <div className="brand brand--sidebar">
        <img className="brand__logo" src={smartSolarLogo} alt="Smart Solar" />
        <small>Microgrid Trading System</small>
      </div>
      <nav className="sidebar__nav">
        <p className="sidebar__section-label">Workspace</p>
        {visibleItems.map((item) => item.available ? (
          <NavLink key={item.path} to={item.path} end={item.path === '/'} onClick={onNavigate} className={({ isActive }) => `sidebar__link${isActive ? ' sidebar__link--active' : ''}`}>
            <span className="sidebar__link-icon" aria-hidden="true">{item.shortLabel}</span>
            {item.label}
          </NavLink>
        ) : (
          <span key={item.path} className="sidebar__link sidebar__link--unavailable" aria-disabled="true" title={item.unavailableTitle ?? 'Unavailable'}>
            <span className="sidebar__link-icon" aria-hidden="true">{item.shortLabel}</span>
            <span>{item.label}<small>{item.unavailableLabel ?? 'Unavailable'}</small></span>
          </span>
        ))}
      </nav>
      <p className="sidebar__note">Navigation is filtered for your authenticated role.</p>
    </aside>
  )
}
