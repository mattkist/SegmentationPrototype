import { NavLink, Outlet } from 'react-router-dom'
import { Leaf, Users, Table2, SlidersHorizontal, FlaskConical, Home } from 'lucide-react'
import { cn } from '../lib/cn'
import { Hint } from './Hint'
import { hints } from '../hints/en'
import { useCropSeason } from '../context/CropSeasonContext'

const nav: {
  to: string
  label: string
  icon: typeof Home
  end?: boolean
}[] = [
  { to: '/', label: 'Overview', icon: Home, end: true },
  { to: '/farmers', label: 'Farmers', icon: Users },
  { to: '/kpis', label: 'KPI data', icon: Table2 },
  { to: '/configurations', label: 'Configurations', icon: SlidersHorizontal },
  { to: '/simulations', label: 'Simulations', icon: FlaskConical },
]

export function AppLayout() {
  const { seasons, seasonId, setSeasonId, isLoading } = useCropSeason()

  return (
    <div className="flex min-h-screen">
      <aside className="sticky top-0 flex h-screen w-56 shrink-0 flex-col border-r border-black/5 bg-surface-card shadow-card">
        <div className="flex items-center gap-2 border-b border-black/5 px-4 py-4">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-leaf text-white">
            <Leaf className="h-5 w-5" />
          </div>
          <div>
            <div className="font-display text-sm font-semibold leading-tight text-ink">
              Segmentation
            </div>
            <div className="text-[10px] font-medium uppercase tracking-wide text-ink-faint">
              Prototype
            </div>
          </div>
        </div>
        <nav className="flex flex-1 flex-col gap-0.5 p-2">
          {nav.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={Boolean(end)}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition',
                  isActive
                    ? 'bg-leaf-soft text-leaf'
                    : 'text-ink-muted hover:bg-surface-muted hover:text-ink',
                )
              }
            >
              <Icon className="h-4 w-4 shrink-0 opacity-80" />
              {label}
            </NavLink>
          ))}
        </nav>
        <div className="border-t border-black/5 p-3 text-[11px] leading-snug text-ink-faint">
          Tooltips explain scoring rules and CSV shapes. API runs on port 5130 by default.
        </div>
      </aside>
      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-40 flex flex-wrap items-center justify-between gap-3 border-b border-black/5 bg-surface-card/95 px-6 py-3 backdrop-blur">
          <div className="flex min-w-0 items-center gap-2">
            <span className="text-sm font-medium text-ink-muted">Crop season</span>
            <Hint content={hints.cropSeason} />
            <select
              className="rounded-lg border border-black/10 bg-white px-3 py-1.5 text-sm font-medium text-ink shadow-sm focus:border-leaf focus:outline-none focus:ring-2 focus:ring-leaf/25"
              disabled={isLoading || seasons.length === 0}
              value={seasonId ?? ''}
              onChange={(e) => setSeasonId(Number(e.target.value))}
            >
              {seasons.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.code} ({s.id})
                </option>
              ))}
            </select>
          </div>
        </header>
        <main className="flex-1 px-6 py-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
