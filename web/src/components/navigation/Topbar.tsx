import { Button } from '@/components/ui/Button'

export interface TopbarProps {
  title: string
  onMenuClick: () => void
  menuOpen: boolean
  userName: string
  roleLabel: string
  onLogout: () => void
}

export function Topbar({ title, onMenuClick, menuOpen, userName, roleLabel, onLogout }: TopbarProps) {
  return (
    <header className="topbar">
      <div className="topbar__context">
        <Button id="navigation-menu-button" className="topbar__menu" variant="ghost" onClick={onMenuClick} aria-label="Open navigation menu" aria-controls="app-sidebar" aria-expanded={menuOpen}>
          <span aria-hidden="true">☰</span>
        </Button>
        <span className="topbar__title">{title}</span>
      </div>
      <div className="topbar__user">
        <span className="topbar__avatar" aria-hidden="true">{userName.charAt(0).toUpperCase()}</span>
        <span className="topbar__user-label"><strong>{userName}</strong><small>{roleLabel}</small></span>
        <Button variant="ghost" onClick={onLogout}>Log out</Button>
      </div>
    </header>
  )
}
