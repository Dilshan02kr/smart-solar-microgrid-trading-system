import { NavLink } from 'react-router-dom'

export function ProsumerSubnavigation() {
  return (
    <nav className="subnavigation" aria-label="Prosumer administration">
      <NavLink to="/prosumers" end className={({ isActive }) => `subnavigation__link${isActive ? ' subnavigation__link--active' : ''}`}>All Prosumers</NavLink>
      <NavLink to="/prosumers/pending" className={({ isActive }) => `subnavigation__link${isActive ? ' subnavigation__link--active' : ''}`}>Pending Registrations</NavLink>
    </nav>
  )
}
